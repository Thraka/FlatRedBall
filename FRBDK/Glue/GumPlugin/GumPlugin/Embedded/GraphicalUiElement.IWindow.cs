﻿using FlatRedBall.Gui;
using Gum.Wireframe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Wireframe
{
    public partial class GraphicalUiElement : IWindow
    {

        public event WindowEvent Click;
        public event WindowEvent ClickNoSlide;
        public event WindowEvent SlideOnClick;
        public event WindowEvent Push;
        public event WindowEvent DragOver;
        public event WindowEvent RollOn;
        public event WindowEvent RollOff;
        public event WindowEvent RollOver;

        public event WindowEvent LosePush;

        // Maybe we'll eventually move this out of IWindow implementation into its own file:
        public virtual void AssignReferences()
        {

        }

        bool IsComponentOrInstanceOfComponent()
        {
            if (Tag is Gum.DataTypes.ComponentSave)
            {
                return true;
            }
            else if (Tag is Gum.DataTypes.InstanceSave)
            {
                var instance = Tag as Gum.DataTypes.InstanceSave;

                if (
                    instance.BaseType == "ColoredRectangle" ||
                    instance.BaseType == "Container" ||
                    instance.BaseType == "NineSlice" ||

                    instance.BaseType == "Sprite" ||
                    instance.BaseType == "Text")
                {
                    return false;
                }
                else
                {
                    // If we got here, then it's a component
                    return true;
                }
            }
            return false;
        }

        protected virtual void CallCustomInitialize()
        {

        }

        partial void CustomAddToManagers()
        {
            if (IsComponentOrInstanceOfComponent() && this.Parent == null)
            {
                GuiManager.AddWindow(this);
            }
            CallCustomInitialize();
        }

        partial void CustomRemoveFromManagers()
        {
            if (IsComponentOrInstanceOfComponent())
            {
                GuiManager.RemoveWindow(this);
            }
        }

        #region IWindow implementation

        public void Activity(FlatRedBall.Camera camera)
        {

        }

        public void CallClick()
        {
            if (this.Click != null)
            {
                Click(this);
            }
        }

        public void CallRollOff()
        {
            if (this.RollOff != null)
            {
                RollOff(this);
            }
        }

        public void CallRollOver()
        {
            if(this.RollOver != null)
            {
                RollOver(this);
            }
        }

        public void CallRollOn()
        {
            if (this.RollOn != null)
            {
                RollOn(this);
            }
        }

        void CallLosePush()
        {
            if (LosePush != null)
            {
                LosePush(this);
            }
        }

        System.Collections.ObjectModel.ReadOnlyCollection<IWindow> IWindow.Children
        {
            get { throw new NotImplementedException(); }
        }

        public void CloseWindow()
        {
            throw new NotImplementedException();
        }

        private bool mEnabled = true;

        public bool Enabled
        {
            get
            {
                return mEnabled;
            }
            set
            {
                mEnabled = value;
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<IWindow> FloatingChildren
        {
            get { return null; }
        }

        public bool GetParentVisibility()
        {
            throw new NotImplementedException();
        }

        public bool GuiManagerDrawn
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IgnoredByCursor
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsPointOnWindow(float x, float y)
        {
            throw new NotImplementedException();
        }

        public bool MovesWhenGrabbed
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void OnDragging()
        {
            if (this.DragOver != null)
            {
                DragOver(this);
            }
        }

        public void OnLosingFocus()
        {

        }

        public void OnResize()
        {
            throw new NotImplementedException();
        }

        public void OnResizeEnd()
        {
            throw new NotImplementedException();
        }

        public bool OverlapsWindow(IWindow otherWindow)
        {
            throw new NotImplementedException();
        }

        IWindow IWindow.Parent
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public float ScaleX
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public float ScaleY
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void SetScaleTL(float newScaleX, float newScaleY, bool keepTopLeftStatic)
        {
            throw new NotImplementedException();
        }

        public void SetScaleTL(float newScaleX, float newScaleY)
        {
            throw new NotImplementedException();
        }

        public FlatRedBall.ManagedSpriteGroups.SpriteFrame SpriteFrame
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        
        /// <summary>
        /// Tries to handle cursor activity. If this returns true, then either this element or one of its
        /// children handled the activity. 
        /// </summary>
        /// <param name="cursor">Reference to the cursor object</param>
        /// <returns>Whether this or one of its children handled the cursor activity, blocking other windows from receiving cursor input this frame.</returns>
        /// <remarks>This method will always allow children to handle the activity first, as children draw in front of their parents. Only components
        /// can have UI elements. Standard elements such as Sprites or Containers cannot themselves handle the activity, but they do give their children the
        /// opportunity to handle activity. This is because components (such as buttons) may be part of a container for stacking or other organization.
        /// 
        /// Ultimately this hierarchical logic exists because only the top-most parent is added to the GuiManager, and it is responsible for
        /// giving its children the opportunity to perform cursor-related input. </remarks>
        private bool TryHandleCursorActivity(Cursor cursor)
        {
            bool handledByChild = false;
            bool handledByThis = false;

            if (HasCursorOver(cursor))
            {

                #region Try handling by children

                // Let's see if any children have the cursor over:
                for(int i = this.Children.Count -1; i > -1; i--)
                {
                    var child = this.Children[i];
                    if (child is GraphicalUiElement)
                    {
                        var asGue = child as GraphicalUiElement;
                        // Children should always have the opportunity to handle activity,
                        // even if they are not components, because they may contain components as their children
                        //if (asGue.IsComponentOrInstanceOfComponent() && asGue.HasCursorOver(cursor))
                        if (asGue.HasCursorOver(cursor))
                            {
                            handledByChild = asGue.TryHandleCursorActivity(cursor);

                            if(handledByChild)
                            {
                                break;
                            }
                        }
                    }
                }

                #endregion


                if (!handledByChild && this.IsComponentOrInstanceOfComponent())
                {
                    handledByThis = true;

                    cursor.WindowOver = this;

                    if (cursor.PrimaryPush)
                    {

                        cursor.WindowPushed = this;

                        if (Push != null)
                            Push(this);


                        cursor.GrabWindow(this);

                    }

                    if (cursor.PrimaryClick) // both pushing and clicking can occur in one frame because of buffered input
                    {
                        if (cursor.WindowPushed == this)
                        {
                            if (Click != null)
                            {
                                Click(this);
                            }
                            if (cursor.PrimaryClickNoSlide && ClickNoSlide != null)
                            {
                                ClickNoSlide(this);
                            }

                            // if (cursor.PrimaryDoubleClick && DoubleClick != null)
                            //   DoubleClick(this);
                        }
                        else
                        {
                            if (SlideOnClick != null)
                            {
                                SlideOnClick(this);
                            }
                        }
                    }
                }
            }

            return handledByThis || handledByChild;
        }


        public void TestCollision(Cursor cursor)
        {
            TryHandleCursorActivity(cursor);
        }

        public void UpdateDependencies()
        {
        }


        public float WorldUnitRelativeX
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public float WorldUnitRelativeY
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public float WorldUnitX
        {
            get
            {
                return this.AbsoluteX;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public float WorldUnitY
        {
            get
            {
                return this.AbsoluteY;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        
        public bool HasCursorOver(Cursor cursor)
        {
            if (((IWindow)this).AbsoluteVisible)
            {
                int screenX = cursor.ScreenX;
                int screenY = cursor.ScreenY;

                float worldX;
                float worldY;

                this.Managers.Renderer.Camera.ScreenToWorld(
                    screenX, screenY,
                    out worldX, out worldY);


                // for now we'll just rely on the bounds of the GUE itself

                return global::RenderingLibrary.IPositionedSizedObjectExtensionMethods.HasCursorOver(
                    this, worldX, worldY);
            }
            else
            {
                return false;
            }
        }

        FlatRedBall.Graphics.Layer FlatRedBall.Graphics.ILayered.Layer
        {
            get { return null; }
        }

        #endregion

        FlatRedBall.Graphics.IVisible FlatRedBall.Graphics.IVisible.Parent
        {
            get { return this.Parent as FlatRedBall.Graphics.IVisible; }
        }

        bool FlatRedBall.Graphics.IVisible.AbsoluteVisible
        {
            get { return mContainedObjectAsIVisible != null && mContainedObjectAsIVisible.AbsoluteVisible; }
        }

        bool FlatRedBall.Graphics.IVisible.IgnoresParentVisibility
        {
            get;
            set;
        }


        public void MoveToFrbLayer(FlatRedBall.Graphics.Layer layer, FlatRedBall.Gum.GumIdb containingScreen)
        {
            var gumLayer = containingScreen.GumLayersOnFrbLayer(layer).FirstOrDefault();

            if (gumLayer != null)
            {
                this.MoveToLayer(gumLayer);
            }
#if DEBUG
            else
            {
                throw new Exception("There is no associated Gum layer for the FRB Layer " + layer);
            }
#endif
        }

        public void Destroy()
        {
            this.RemoveFromManagers();
            StopAnimations();
        }

        public virtual void StopAnimations()
        {
        }

        public FlatRedBall.Glue.StateInterpolation.Tweener InterpolateTo(Gum.DataTypes.Variables.StateSave first, Gum.DataTypes.Variables.StateSave second, double secondsToTake, FlatRedBall.Glue.StateInterpolation.InterpolationType interpolationType, FlatRedBall.Glue.StateInterpolation.Easing easing)
        {
            FlatRedBall.Glue.StateInterpolation.Tweener tweener = new FlatRedBall.Glue.StateInterpolation.Tweener(from: 0, to: 1, duration: (float)secondsToTake, type: interpolationType, easing: easing);
            tweener.Owner = this;
            tweener.PositionChanged = newPosition => this.InterpolateBetween(first, second, newPosition);
            tweener.Start();
            StateInterpolationPlugin.TweenerManager.Self.Add(tweener);
            return tweener;
        }

    }
}
