using Discord;
using System.Collections.Generic;
using System.Linq;

namespace FluffyMusic.Core.ExtensionMethods
{
    public static class ExperimentalExtensions
    {
        //  Experimental
        public static MessageComponent RemoveButtonFromRow(this IReadOnlyCollection<ActionRowComponent> components, string customId)
        {
            if(components.Count == 0)
            {
                return null;
            }

            ComponentBuilder builder = new ComponentBuilder();
            builder.ActionRows = new List<ActionRowBuilder>();

            foreach (var row in components)
            {
                ActionRowBuilder rowBuilder = new ActionRowBuilder()
                    .WithComponents(row.Components.Where(c => c.CustomId != customId).ToList());
                if(rowBuilder.Components.Count > 0)
                {
                    builder.ActionRows.Add(rowBuilder);
                }
            }

            return builder.ActionRows.Count > 0
                ? builder.Build()
                : null;
        }
    }
}
