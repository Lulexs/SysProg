namespace Projekat1.Utils;

public class Trie {
    private readonly TrieNode _root = new TrieNode();

    public void BuildTrie(IEnumerable<string> patterns) {
        foreach (var pattern in patterns) {
            Insert(pattern);
        }
        AddFailureLinks();
    }

    private void AddFailureLinks() {
        Queue<TrieNode> queue = new();
        foreach (var child in _root.ChildrenMap.Values) {
            child.FailureLink = null;
            queue.Enqueue(child);
        }

        while (queue.Count > 0) {
            var currentNode = queue.Dequeue();
            foreach (KeyValuePair<char, TrieNode> child in currentNode.ChildrenMap) {
                queue.Enqueue(child.Value);
                var failureNode = currentNode.FailureLink;
                while (failureNode != null && !failureNode.ChildrenMap.ContainsKey(child.Key)) {
                    failureNode = failureNode.FailureLink;
                }

                child.Value.FailureLink = failureNode ?? _root;
            }
        }
    }

    private void Insert(string word) {
        var cur = _root;
        foreach (var c in word) {
            if (!cur.ChildrenMap.ContainsKey(c)) {
                cur.ChildrenMap[c] = new TrieNode();
            }

            cur = cur.ChildrenMap[c];
        }
        cur.IsWord = true;
    }

    
}