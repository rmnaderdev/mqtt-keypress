using System;
using System.Linq;
using System.Threading;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

class Program
{

    static void Main(string[] args)
    {
        // Use your client ID from Discord's developer site.
        var clientID = "669971537897717780";

        var discord = new Discord.Discord(Int64.Parse(clientID), (UInt64)Discord.CreateFlags.Default);
        discord.SetLogHook(Discord.LogLevel.Debug, (level, message) =>
        {
            Console.WriteLine("Log[{0}] {1}", level, message);
        });

        var voiceManager = discord.GetVoiceManager();
        voiceManager.OnSettingsUpdate += () =>
        {
            var deafened = voiceManager.IsSelfDeaf();
            var muted = voiceManager.IsSelfMute();

            //Task.Run(async () =>
            //{
            //    await Task.Delay(1000);
            //    voiceManager.SetSelfDeaf(!deafened);
            //});

            Console.WriteLine("settings updated, deafened={0}, muted={1}", deafened, muted);
        };


        // Pump the event look to ensure all callbacks continue to get fired.
        try
        {
            while (true)
            {
                discord.RunCallbacks();
                //lobbyManager.FlushNetwork();
                Thread.Sleep(1000 / 60);
            }
        }
        finally
        {
            discord.Dispose();
        }

    }
}
