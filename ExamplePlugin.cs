using Nitrox_PublixExtension.Core;
using Nitrox_PublixExtension.Core.Plugin;
using Nitrox_PublixExtension.Core.Plugin.Attributes;
using NitroxModel.Logger;
using Publix_ExamplePlugin.EventListeners;

namespace Publix_ExamplePlugin
{
    [PluginInfo("dev.drperky.exampleplugin", "Example Plugin", "0.2", "1.0.2")]
    [PluginDescription("Example Plugin For PublixExtension")] //Description isnt used at the time of writing this, so its optional
    public class ExamplePlugin : BasePlugin
    {
        public class ExampleConfig
        {
            public string ExampleString { get; set; } = "DefaultValue";
            public bool ExampleBool { get; set; } = true;
            public float ExampleFloat { get; set; } = float.MaxValue;
        }

        ExampleConfig loadedConfig;

        public override void OnEnable()
        {
            Publix.getEventManager().Register(this, new ExampleEventListener(this));

            loadedConfig = this.GetConfigManager().GetConfig<ExampleConfig>();

            this.GetLogger().Info($"Config Value: {loadedConfig.ExampleString}");

            this.GetLogger().Info("Plugin Loaded :)");
        }
        public override void OnDisable()
        {
            this.GetConfigManager().SaveConfig(loadedConfig);
        }
    }
}
