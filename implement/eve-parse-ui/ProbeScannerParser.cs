namespace eve_parse_ui
{
    internal class ProbeScannerParser
    {
        public static ProbeScannerWindow? ParseProbeScannerWindowFromUITreeRoot(UITreeNodeNoDisplayRegion uiTreeRoot)
        {
            var windowNode = uiTreeRoot.GetDescendantsByType("ProbeScannerWindow").FirstOrDefault();

            if (windowNode == null)
                return null;

            var scanResults = windowNode
                .GetDescendantsByType("ScanResultNew")
                .Select(ParseProbeScanResult)
                .ToList();

            return new ProbeScannerWindow
            {
                UiNode = windowNode,
                ScanResults = scanResults,
                ScrollingPanel = UIParser.ParseScrollingPanel(windowNode)
            };
        }

        public static ProbeScanResult ParseProbeScanResult(
            UITreeNodeWithDisplayRegion scanResultNode
        )
        {
            var detailsLeftToRight = scanResultNode
                .GetDescendantsByType("EveLabelMedium")
                .ToList();

            var warpButton = scanResultNode
                .ListDescendantsWithDisplayRegion()
                .FirstOrDefault(node =>
                    node.GetTexturePathFromDictEntries()?.EndsWith("44_32_18.png") == true);

            string? signal = null;
            string? group = null;
            if (warpButton == null)
            {
                signal = UIParser.GetDisplayText(detailsLeftToRight.Last());
                group = detailsLeftToRight.Count == 5 ? UIParser.GetDisplayText(detailsLeftToRight[3]) : null;
            }
            else
            {
                group = detailsLeftToRight.Count == 4 ? UIParser.GetDisplayText(detailsLeftToRight[3]) : null;

                // adjust the width of the scanResultNode to account for the warp button
                scanResultNode.TotalDisplayRegionVisible = new DisplayRegion(
                    scanResultNode.TotalDisplayRegionVisible.X,
                    scanResultNode.TotalDisplayRegionVisible.Y,
                    scanResultNode.TotalDisplayRegionVisible.Width - warpButton.TotalDisplayRegionVisible.Width,
                    scanResultNode.TotalDisplayRegionVisible.Height
                );
            }

            var distance = UIParser.GetDisplayText(detailsLeftToRight[0]);
            var id = UIParser.GetDisplayText(detailsLeftToRight[1]);
            var name = UIParser.GetDisplayText(detailsLeftToRight[2]);

            return new ProbeScanResult
            {
                UiNode = scanResultNode,
                Distance = distance ?? "??",
                ID = id ?? "??",
                Name = name ?? "??",
                Group = group,
                Signal = signal,
                WarpButton = warpButton
            };
        }
    }
}
