namespace Projekat1.Utils;

public class TrieNode {
    public Dictionary<char, TrieNode> ChildrenMap { get; }
    public bool IsWord { get; set; }

    public TrieNode? FailureLink = null;

    public TrieNode() {
        ChildrenMap = new Dictionary<char, TrieNode>();
    }
    
}