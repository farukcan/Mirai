using Mirai.Models;
using Mirai.Services;
using Serilog;

namespace Mirai.Controls
{
    public static class RssBot
    {
        public static void Load(){
            // AddTelegramGroupId
            Guideline.Commands.Add(
                Command.Builder
                    .SetName("Add Telegram Group Id")
                    .SetDescription("Adds Telegram group id with a name")
                    .SetFunction("AddTelegramGroupId(groupName,groupId)")
                    .SetParameter("groupName" , "Group Name")
                    .SetParameter("groupId" , "Group Identifier")
                    .SetHandler(AddGroupId)
                    .Build()
            );
            // RemoveTelegramGroup
            Guideline.Commands.Add(
                Command.Builder
                    .SetName("Remove Telegram Group")
                    .SetDescription("Removes Telegram group")
                    .SetFunction("RemoveTelegramGroup(groupName)")
                    .SetParameter("groupName" , "Group Name")
                    .SetHandler(RemoveGroup)
                    .Build()
            );
            // TrackFeed
            Guideline.Commands.Add(
                Command.Builder
                    .SetName("Track Feed")
                    .SetDescription("Tracks a RSS feed")
                    .SetFunction("TrackFeed(feedName,feedUrl)")
                    .SetParameter("feedName" , "Feed Name")
                    .SetParameter("feedUrl" , "Feed Url")
                    .SetHandler(TrackFeed)
                    .Build()
            );
            // RemoveFeed
            Guideline.Commands.Add(
                Command.Builder
                    .SetName("Remove Feed")
                    .SetDescription("Removes a RSS feed")
                    .SetFunction("RemoveFeed(feedName)")
                    .SetParameter("feedName" , "Feed Name")
                    .SetHandler(RemoveFeed)
                    .Build()
            );
        }

        private static async Task RemoveFeed(Dialog dialog, string[] args)
        {
            if(Bot.Instance is null) return;
            var infos = Bot.Instance.rethink.Linq<Information>("Mirai","Informations")
                            .OrderByDescending(i=>i.Time)
                            .ToArray()
                            .Where(i=>
                                i.Type == "FeedDefinition"
                                && i.Tags.Contains(args[0])
                            ).ToArray();
            
            Log.Information($"Feeds found : {infos.Length}");

            if(infos.Length == 0){
                await Bot.Instance.Answer(dialog, $"Feed '{args[0]}' not found");
                return;
            }
            foreach(var info in infos){
                await Bot.Instance.DeleteInformation(info);
            }
            await Bot.Instance.Answer(dialog, $"Feed '{args[0]}' removed. Count : {infos.Length}");
        }

        private static async Task TrackFeed(Dialog dialog, string[] args)
        {
            if(Bot.Instance is null) return;
            // Create Information
            var data = $"FeedName={args[0]};FeedUrl={args[1]}";
            var info = new Information("FeedDefinition", dialog.Interlocutor, data);
            info.AddTag("FeedDefinition");
            info.AddTag(args[0]);
            await Bot.Instance.CreateInformation(info);
            Log.Information($"Feed Added : {data}");
            await Bot.Instance.Answer(dialog, $"Feed Added : {data}");
        }

        private static async Task AddGroupId(Dialog dialog, string[] args)
        {
            if(Bot.Instance is null) return;
            // Create Information
            var data = $"GroupName={args[0]};GroupId={args[1]}";
            var info = new Information("GroupDefinition", dialog.Interlocutor, data);
            info.AddTag("GroupDefinition");
            info.AddTag(args[0]);
            await Bot.Instance.CreateInformation(info);
            Log.Information($"Group Added : {data}");
            await Bot.Instance.Answer(dialog, $"Group Added : {data}");
        }
        private static async Task RemoveGroup(Dialog dialog, string[] args){
            if(Bot.Instance is null) return;
            var infos = Bot.Instance.rethink.Linq<Information>("Mirai","Informations")
                            .OrderByDescending(i=>i.Time)
                            .ToArray()
                            .Where(i=>
                                i.Type == "GroupDefinition"
                                && i.Tags.Contains(args[0])
                            ).ToArray();
            
            Log.Information($"Groups found : {infos.Length}");

            if(infos.Length == 0){
                await Bot.Instance.Answer(dialog, $"Group '{args[0]}' not found");
                return;
            }
            foreach(var info in infos){
                await Bot.Instance.DeleteInformation(info);
            }
            await Bot.Instance.Answer(dialog, $"Group '{args[0]}' removed. Count : {infos.Length}");
        }
    }
}