// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Utcp.Core.Search;

using System.Text.RegularExpressions;
using Utcp.Core.Interfaces;
using Utcp.Core.Models;

public sealed class TagAndDescriptionWordMatchStrategy : IToolSearchStrategy
{
    public double DescriptionWeight { get; init; } = 1;
    public double TagWeight { get; init; } = 3;

    public async Task<IReadOnlyList<Tool>> SearchToolsAsync(IConcurrentToolRepository toolRepository, string query, int limit = 10, IReadOnlyList<string>? anyOfTagsRequired = null, CancellationToken cancellationToken = default)
    {
        if (limit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit));
        }

        var tools = await toolRepository.GetToolsAsync(cancellationToken).ConfigureAwait(false);

        if (anyOfTagsRequired is not null && anyOfTagsRequired.Count > 0)
        {
            var required = anyOfTagsRequired.Select(t => t.ToLowerInvariant()).ToHashSet();
            tools = tools.Where(t => t.Tags.Any(tag => required.Contains(tag.ToLowerInvariant()))).ToList();
        }

        var queryLower = query.ToLowerInvariant();
        var queryWords = Regex.Matches(queryLower, "\\w+").Select(m => m.Value).ToHashSet();

        var scored = new List<(Tool tool, double score)>();
        foreach (var tool in tools)
        {
            double score = 0;

            foreach (var tag in tool.Tags)
            {
                var tagLower = tag.ToLowerInvariant();
                if (queryLower.Contains(tagLower))
                {
                    score += this.TagWeight;
                    continue;
                }

                var tagWords = Regex.Matches(tagLower, "\\w+").Select(m => m.Value).ToHashSet();
                if (tagWords.Any(w => queryWords.Contains(w)))
                {
                    score += this.TagWeight;
                }
            }

            if (!string.IsNullOrWhiteSpace(tool.Description))
            {
                var descriptionWords = Regex.Matches(tool.Description.ToLowerInvariant(), "\\w+").Select(m => m.Value).ToHashSet();
                foreach (var word in descriptionWords)
                {
                    if (word.Length > 2 && queryWords.Contains(word))
                    {
                        score += this.DescriptionWeight;
                    }
                }
            }

            scored.Add((tool, score));
        }

        var sorted = scored.OrderByDescending(s => s.score).Select(s => s.tool);
        return limit == 0 ? sorted.ToList() : sorted.Take(limit).ToList();
    }
}

