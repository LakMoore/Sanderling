using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static eve_parse_ui.UITreeNodeWithDisplayRegion;

namespace eve_parse_ui
{
    internal class ProbeScannerParser
    {
        public static ProbeScannerWindow? ParseProbeScannerWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
        {
            var windowNode = uiTreeRoot.GetDescendantsByType("ProbeScannerWindow").FirstOrDefault();

            if (windowNode == null)
                return null;

            var scanResultsNodes = windowNode
                .GetDescendantsByType("ScanResultNew")
                .ToList();

            var scrollNode = windowNode
                .ListDescendantsWithDisplayRegion()
                .Where(node => node.GetNameFromDictEntries()
                    ?.Contains("ResultsContainer") ?? false)
                .SelectMany(node => node.ListDescendantsWithDisplayRegion())
                .FirstOrDefault(node => node.pythonObjectTypeName.Contains("scroll", StringComparison.CurrentCultureIgnoreCase));

            var headersContainerNode = scrollNode;
            var entriesHeaders = headersContainerNode?.GetAllContainedDisplayTextsWithRegion() ?? [];

            var scanResults = scanResultsNodes
                .Select(node => ParseProbeScanResult(entriesHeaders, node))
                .ToList();

            return new ProbeScannerWindow
            {
                UiNode = windowNode,
                ScanResults = scanResults
            };
        }

        public static ProbeScanResult ParseProbeScanResult(
            List<DisplayTextWithRegion> entriesHeaders,
            UITreeNodeWithDisplayRegion scanResultNode)
        {
            var textsLeftToRight = scanResultNode
                .GetAllContainedDisplayTextsWithRegion()?
                .OrderBy(x => x.Region.TotalDisplayRegion.X)
                .Select(x => x.Text)
                .ToList();

            var cellsTexts = scanResultNode
                .GetAllContainedDisplayTextsWithRegion()?
                .Select(cell =>
                {
                    var cellMiddle = cell.Region.TotalDisplayRegion.X + (cell.Region.TotalDisplayRegion.Width / 2);

                    var matchingHeader = entriesHeaders
                        .FirstOrDefault(header =>
                        {
                            var headerRegion = header.Region.TotalDisplayRegion;
                            return headerRegion.X < cellMiddle + 1 &&
                                   cellMiddle < headerRegion.X + headerRegion.Width - 1;
                        });

                    return matchingHeader != default
                        ? (matchingHeader.Text, cell.Text)
                        : ((string, string)?)null;
                })
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToDictionary(x => x.Item1, x => x.Item2);

            var warpButton = scanResultNode
                .ListDescendantsWithDisplayRegion()
                .FirstOrDefault(node =>
                    node.GetTexturePathFromDictEntries()?.EndsWith("44_32_18.png") == true);

            return new ProbeScanResult
            {
                UiNode = scanResultNode,
                TextsLeftToRight = textsLeftToRight,
                CellsTexts = cellsTexts,
                WarpButton = warpButton
            };
        }
    }
}
