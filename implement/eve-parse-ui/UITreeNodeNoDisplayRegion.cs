using read_memory_64_bit;
using System.Xml.Linq;

namespace eve_parse_ui
{
    public record UITreeNodeNoDisplayRegion : UITreeNode
    {
        public UITreeNodeNoDisplayRegion? Parent { get; set; }

        public required IReadOnlyList<UITreeNodeNoDisplayRegion> Children { get; init; }

        public UITreeNodeNoDisplayRegion(UITreeNode original) : base(
            original.pythonObjectAddress,
            original.pythonObjectTypeName,
            original.dictEntriesOfInterest,
            original.otherDictEntriesKeys,
            original.children
        )
        {
            // children should have been completely converted to Children
            // comment the following line if you want to check
            this.children = [];
        }

        public List<UITreeNodeNoDisplayRegion> ListDescendantsInUITreeNode()
        {
            var result = new List<UITreeNodeNoDisplayRegion>();

            if (this.Children != null)
            {
                foreach (var child in this.Children)
                {
                    if (child == null) continue;
                    result.Add(child);
                    result.AddRange(child.ListDescendantsInUITreeNode());
                }
            }

            return result;
        }

        public IEnumerable<UITreeNodeWithDisplayRegion> ListDescendantsWithDisplayRegion()
        {
            if (this.Children == null) yield break;

            foreach (var child in this.Children)
            {
                if (child is UITreeNodeWithDisplayRegion childWithRegion)
                {
                    foreach (var descendant in childWithRegion.ListDescendantsWithDisplayRegionIncludingSelf())
                    {
                        yield return descendant;
                    }
                }
            }
        }

        private IEnumerable<UITreeNodeWithDisplayRegion> ListDescendantsWithDisplayRegionIncludingSelf()
        {
            if (this is UITreeNodeWithDisplayRegion node)
                yield return node;

            if (this.Children == null) yield break;

            foreach (var child in this.Children)
            {
                if (child is UITreeNodeWithDisplayRegion childWithRegion)
                {
                    foreach (var descendant in childWithRegion.ListDescendantsWithDisplayRegionIncludingSelf())
                    {
                        yield return descendant;
                    }
                }
            }
        }

        // Helper function to get nodes by type
        public IEnumerable<UITreeNodeWithDisplayRegion> GetDescendantsByType(string typeName) =>
            this
                .ListDescendantsWithDisplayRegion()
                .Where(node => node.pythonObjectTypeName.Equals(typeName, StringComparison.CurrentCultureIgnoreCase));

        // Helper function to get nodes by type
        public IEnumerable<UITreeNodeWithDisplayRegion> GetDescendantsByName(string name) =>
            this
                .ListDescendantsWithDisplayRegion()
                .Where(node => node.GetNameFromDictEntries()?.Equals(name, StringComparison.CurrentCultureIgnoreCase) == true);

        public string? GetNameFromDictEntries()
        {
            return dictEntriesOfInterest.GetValueOrDefault("_name") as string;
        }
    }
}
