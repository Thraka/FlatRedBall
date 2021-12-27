﻿using FlatRedBall.Glue.Controls;
using FlatRedBall.Glue.Plugins.ExportedImplementations;
using FlatRedBall.Glue.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FlatRedBall.Glue.Controls
{
    public class AddScreenWindow : CustomizableTextInputWindow
    {
        GlueFormsCore.ViewModels.AddScreenViewModel ViewModel => DataContext as GlueFormsCore.ViewModels.AddScreenViewModel;
        public IReadOnlyCollection<UserControl> UserControlChildren
        {
            get
            {
                var listToReturn = new List<UserControl>();

                var uiElements = AboveTextBoxStackPanel.Children.Where(item => item is UserControl);
                foreach (var element in uiElements)
                {
                    listToReturn.Add(element as UserControl);
                }

                uiElements = BelowTextBoxStackPanel.Children.Where(item => item is UserControl);
                foreach (var element in uiElements)
                {
                    listToReturn.Add(element as UserControl);
                }


                return listToReturn;
            }
        }

        public AddScreenWindow() : base()
        {
            Width = 500;

            var binding = new Binding(nameof(ViewModel.ScreenName));
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            this.TextBox.SetBinding(TextBox.TextProperty, binding);
            this.ValidationLabel.SetBinding(TextBlock.TextProperty, nameof(ViewModel.NameValidationMessage));
            this.ValidationLabel.SetBinding(TextBlock.VisibilityProperty, nameof(ViewModel.ValidationVisibility));

            CustomOkClicked += (not, used) =>
            {
                if(!string.IsNullOrEmpty( ViewModel.NameValidationMessage))
                {
                    GlueCommands.Self.DialogCommands.ShowMessageBox(ViewModel.NameValidationMessage);
                }
                else
                {
                    this.DialogResult = true;
                }
            };

            this.Loaded += HandleLoaded;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            HighlghtText();
        }
    }
}
