﻿using System;
using System.Threading.Tasks;
using AutoBusy.Windows;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

namespace AutoBusy
{
    public sealed unsafe class Plugin : IDalamudPlugin
    {
        public string Name => "AutoBusy";
        private const string CommandName = "/pbusy";

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public IClientState ClientState { get; init; }
        public WindowSystem WindowSystem = new("AutoBusy");

        private ConfigWindow ConfigWindow { get; init; }

        public Plugin( 
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager,
            [RequiredVersion("1.0")] IClientState clientState,
            IFramework frame)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.ClientState = clientState;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            ConfigWindow = new ConfigWindow(this);
            
            WindowSystem.AddWindow(ConfigWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "/pbusy"
            });
         
            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            clientState.Login += () =>
            {
                if (Configuration.Enabled)
                {
                    Task.Delay(TimeSpan.FromSeconds(5))
                        .ContinueWith(task =>
                            RaptureShellModule.Instance()->ExecuteMacro(RaptureMacroModule.Instance()->GetMacro(1U, 59)));
                }
            };
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            
            ConfigWindow.Dispose();
            
            this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            DrawConfigUI();
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
    }
}
