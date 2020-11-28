﻿using FlatRedBall.Glue.CodeGeneration.CodeBuilder;
using FlatRedBall.Glue.Managers;
using Gum.DataTypes;
using Gum.Managers;
using GumPlugin.CodeGeneration;
using GumPlugin.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GumPluginCore.CodeGeneration
{
    public class FormsClassCodeGenerator : Singleton<FormsClassCodeGenerator>
    {
        public static string FormsRuntimeNamespace =>
            FlatRedBall.Glue.ProjectManager.ProjectNamespace + ".FormsControls";

        public string GenerateCodeFor(ElementSave elementSave)
        {
            if (elementSave == null)
            {
                throw new ArgumentNullException(nameof(elementSave));
            }

            bool shouldGenerate = false;

            var isScreen = elementSave is ScreenSave;
            var isComponent = elementSave is ComponentSave;
            if (isScreen)
            {
                shouldGenerate = true;
            }
            else if(isComponent)
            {
                shouldGenerate = GetIfShouldGenerate(elementSave);
            }
            // don't do anything with standards

            if (shouldGenerate)
            {
                var topBlock = new CodeBlockBase();
                var fullNamespace = GetFullRuntimeNamespaceFor(elementSave);
                var currentBlock = topBlock.Namespace(fullNamespace);
                GenerateScreenAndComponentCodeFor(elementSave, currentBlock);
                return topBlock.ToString();
            }
            else
            {
                return null;
            }
        }

        private bool GetIfShouldGenerate(ElementSave elementSave)
        {
            bool shouldGenerate;
            var component = elementSave as ComponentSave;

            var behaviors = component?.Behaviors;

            string controlType = null;

            if (behaviors != null)
            {
                controlType = GueDerivingClassCodeGenerator.GetFormsControlTypeFrom(behaviors);
            }

            shouldGenerate = controlType == null;

            // todo - see if there are any Forms controls here? Or always generate? Not sure...
            // Update 11/27/2020 - Justin's game has lots of components that aren't forms and this
            // is adding a lot of garbage to Justin's project.
            if (shouldGenerate)
            {
                var allInstances = elementSave.Instances;

                shouldGenerate = allInstances.Any(item =>
                {
                    var instanceElement = ObjectFinder.Self.GetElementSave(item);

                    if(instanceElement is ComponentSave component)
                    {

                        return GueDerivingClassCodeGenerator.GetFormsControlTypeFrom(component.Behaviors) != null ||
                            GetIfShouldGenerate(instanceElement);

                    }
                    return false;
                });
            }

            return shouldGenerate;
        }

        private void GenerateScreenAndComponentCodeFor(ElementSave elementSave, ICodeBlock codeBlock)
        {
            ICodeBlock currentBlock = GenerateClassHeader(codeBlock, elementSave);

            GenerateProperties(elementSave, currentBlock);

            string runtimeClassName = GetUnqualifiedRuntimeTypeFor(elementSave);

            GenerateConstructor(elementSave, currentBlock, runtimeClassName);

            GenerateReactToVisualChanged(elementSave, currentBlock);

            currentBlock.Line("partial void CustomInitialize();");
        }

        private void GenerateConstructor(ElementSave elementSave, ICodeBlock currentBlock, string runtimeClassName)
        {
            string baseCall = null;
            if (elementSave is ComponentSave)
            {
                baseCall = "base(visual)";
            }
            var constructor = currentBlock.Constructor("public", runtimeClassName, "Gum.Wireframe.GraphicalUiElement visual", baseCall);

            if(elementSave is ScreenSave)
            {
                constructor.Line("Visual = visual;");
                constructor.Line("ReactToVisualChanged();");
            }

            constructor.Line("CustomInitialize();");
        }

        private void GenerateReactToVisualChanged(ElementSave elementSave, ICodeBlock currentBlock)
        {
            string methodPre = elementSave is ScreenSave ? "private void" : "protected override void";

            var method = currentBlock.Function(methodPre, "ReactToVisualChanged");
            if(elementSave is ScreenSave || elementSave is ComponentSave)
            {
                foreach (var instance in elementSave.Instances)
                {
                    string type = GetQualifiedRuntimeTypeFor(instance, out bool isStandard);

                    if (!string.IsNullOrEmpty(type))
                    {
                        string line;

                        if(isStandard)
                        {
                            line = 
                                $"{instance.MemberNameInCode()} = ({type})Visual.GetGraphicalUiElementByName(\"{instance.Name}\").FormsControlAsObject;";
                        }
                        else
                        {
                            line =
                                $"{instance.MemberNameInCode()} = new {type}(Visual.GetGraphicalUiElementByName(\"{instance.Name}\"));";
                        }
                        method.Line(line);
                    }
                }
            }

            if(elementSave is ComponentSave)
            {
                method.Line("base.ReactToVisualChanged();");
            }
        }

        private void GenerateProperties(ElementSave elementSave, ICodeBlock currentBlock)
        {
            var rfs = GumProjectManager.Self.GetRfsForGumProject();

            var makePublic =
                true;
                //rfs?.Properties.GetValue<bool>(nameof(GumViewModel.MakeGumInstancesPublic)) == true;

            string publicOrPrivate;
            if (elementSave is Gum.DataTypes.ScreenSave || makePublic)
            {
                // make these public for screens because the only time this will be accessed is in the Glue screen that owns it
                publicOrPrivate = "public";
            }
            else
            {
                publicOrPrivate = "private";
            }

            if(elementSave is ScreenSave)
            {
                currentBlock.Line("private Gum.Wireframe.GraphicalUiElement Visual;");
            }

            foreach (var instance in elementSave.Instances)
            {
                string type = GetQualifiedRuntimeTypeFor(instance, out bool isStandardType);

                if(!string.IsNullOrEmpty(type))
                {
                    ICodeBlock property = currentBlock.AutoProperty($"{publicOrPrivate} " + type, instance.MemberNameInCode());
                }
            }
        }

        private string GetQualifiedRuntimeTypeFor(InstanceSave instance, out bool isStandardForms)
        {
            isStandardForms = false;
            var instanceType = instance.BaseType;
            var component = ObjectFinder.Self.GetComponent(instanceType);

            var behaviors = component?.Behaviors;

            string controlType = null;

            if(behaviors != null)
            {
                controlType = GueDerivingClassCodeGenerator.GetFormsControlTypeFrom(behaviors);
            }

            if(controlType != null)
            {
                isStandardForms = true;
                return controlType;
            }

            // else it may still need to be generated as a reference to a generated form type
            if(component != null && GetIfShouldGenerate(component))
            {
                return GetFullRuntimeNamespaceFor(component) + "." + GetUnqualifiedRuntimeTypeFor(component);
            }

            return null;
        }

        private ICodeBlock GenerateClassHeader(ICodeBlock codeBlock, ElementSave elementSave)
        {
            string runtimeClassName = GetUnqualifiedRuntimeTypeFor(elementSave);

            string inheritance = elementSave is ComponentSave ? "FlatRedBall.Forms.Controls.UserControl" : null;

            ICodeBlock currentBlock = codeBlock.Class("public partial", runtimeClassName, !string.IsNullOrEmpty(inheritance) ? " : " + inheritance : null);
            return currentBlock;
        }

        public string GetUnqualifiedRuntimeTypeFor(ElementSave elementSave)
        {
            return FlatRedBall.IO.FileManager.RemovePath(elementSave.Name) + "Forms";
        }

        public string GetFullRuntimeNamespaceFor(ElementSave elementSave)
        {
            string elementName = elementSave.Name;

            var subfolder = elementSave is ScreenSave ? "Screens" : "Components";

            return GetFullRuntimeNamespaceFor(elementName, subfolder);
        }

        public string GetFullRuntimeNamespaceFor(string elementName, string screensOrComponents)
        {
            string subNamespace;
            if ((elementName.Contains('/')))
            {
                subNamespace = elementName.Substring(0, elementName.LastIndexOf('/')).Replace('/', '.');
            }
            else // if(elementSave is StandardElementSave)
            {
                // can't be in a subfolder
                subNamespace = null;
            }

            if (!string.IsNullOrEmpty(subNamespace))
            {
                subNamespace = '.' + subNamespace;
                subNamespace = subNamespace.Replace(" ", "_");
            }


            var fullNamespace = FormsRuntimeNamespace + "." + screensOrComponents + subNamespace;

            return fullNamespace;
        }
    }
}
