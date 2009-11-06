#pragma checksum "C:\Users\Ahmad\Desktop\CodeBox\Source\CodeBox\Main.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "24A83CD53B04743BC87BF665E2A34EA4"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3053
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using CodeBox.Core.Elements;
using CodeBox.Core.Services.SyntaxValidator;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace CodeBox.Core {
    
    
    public partial class Main : System.Windows.Controls.UserControl {
        
        internal System.Windows.Controls.ScrollViewer ScrollViewerRoot;
        
        internal System.Windows.Controls.Canvas CanvasRoot;
        
        internal System.Windows.Controls.Grid GridRoot;
        
        internal System.Windows.Controls.TextBox textBox;
        
        internal System.Windows.Controls.Border numberPanelBorder;
        
        internal System.Windows.Controls.StackPanel numberPanel;
        
        internal CodeBox.Core.Elements.Paper paper;
        
        internal CodeBox.Core.Services.SyntaxValidator.ErrorTooltip ErrorTooltip;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/CodeBox.Core;component/Main.xaml", System.UriKind.Relative));
            this.ScrollViewerRoot = ((System.Windows.Controls.ScrollViewer)(this.FindName("ScrollViewerRoot")));
            this.CanvasRoot = ((System.Windows.Controls.Canvas)(this.FindName("CanvasRoot")));
            this.GridRoot = ((System.Windows.Controls.Grid)(this.FindName("GridRoot")));
            this.textBox = ((System.Windows.Controls.TextBox)(this.FindName("textBox")));
            this.numberPanelBorder = ((System.Windows.Controls.Border)(this.FindName("numberPanelBorder")));
            this.numberPanel = ((System.Windows.Controls.StackPanel)(this.FindName("numberPanel")));
            this.paper = ((CodeBox.Core.Elements.Paper)(this.FindName("paper")));
            this.ErrorTooltip = ((CodeBox.Core.Services.SyntaxValidator.ErrorTooltip)(this.FindName("ErrorTooltip")));
        }
    }
}
