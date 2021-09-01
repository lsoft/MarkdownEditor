﻿using System;
using System.ComponentModel.Design;
using System.Windows;
using EnvDTE;
using Markdig;
using Microsoft.VisualStudio.Shell;

namespace MarkdownEditor
{
    internal sealed class CopyAsHtmlCommand
    {
        private readonly Package _package;

        private CopyAsHtmlCommand(Package package)
        {
            _package = package;

            var commandService = (OleMenuCommandService)ServiceProvider.GetService(typeof(IMenuCommandService));
            if (commandService != null)
            {
                var cmd = new CommandID(PackageGuids.guidPackageCmdSet, PackageIds.CopyAsHtml);
                var menuItem = new OleMenuCommand(Execute, cmd);
                menuItem.BeforeQueryStatus += BeforeQueryStatus;
                commandService.AddCommand(menuItem);
            }
        }

        public static CopyAsHtmlCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package)
        {
            Instance = new CopyAsHtmlCommand(package);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            button.Visible = button.Enabled = false;

            var document = ProjectHelpers.DTE.ActiveDocument;

            if (MarkdownLanguage.LanguageName.Equals(document?.Language, StringComparison.OrdinalIgnoreCase))
            {
                button.Visible = true;

                var selection = (TextSelection)document.Selection;

                if (!selection.IsEmpty)
                    button.Enabled = true;
            }
        }

        private void Execute(object sender, EventArgs e)
        {
            var document = ProjectHelpers.DTE.ActiveDocument;
            var selection = (TextSelection)document.Selection;
            var markdown = selection.Text;

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var html = Markdown.ToHtml(markdown, pipeline).Replace("\n", Environment.NewLine);

            // TODO: Prettify the HTML

            Clipboard.SetText(html);

            ProjectHelpers.DTE.StatusBar.Text = "HTML copied to clipboard";
        }
    }
}
