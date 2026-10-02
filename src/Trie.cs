
class Trie
{
    class TrieNode
    {
        public SortedDictionary<char, TrieNode> Children {get; } = new SortedDictionary<char, TrieNode>();
        public bool IsEndOfWord { get; set; }
    }

    TrieNode root;
    public Trie() {
        root = new TrieNode();
    }

    public void insert(string key)
    {
        TrieNode current = root;

        foreach(char character in key){
            if(!current.Children.TryGetValue(character, out TrieNode? next))
            {
                next = new TrieNode();
                current.Children[character] = next;
            }
            current = next;
        
        }
        current.IsEndOfWord = true;
    }

    public string GetCompletion(string query)
    {
        if (string.IsNullOrEmpty(query))
            {
                return "";
            }
        // autocomplete the typed command
        TrieNode current = root;
        foreach(char character in query)
        {
            
            if(!current.Children.TryGetValue(character, out current))
            {
                return "";
            }
        }
        return AutoComplete(current, query);
    }
        string AutoComplete(TrieNode root, string currPrefix) {
        // found a string in Trie with the given prefix
        if (root.IsEndOfWord) {
            return currPrefix;
        }
        foreach(KeyValuePair<char, TrieNode> child in root.Children)
        {
            char nextCharacter = child.Key;
            TrieNode nextNode = child.Value;
            string result = AutoComplete(nextNode, currPrefix + nextCharacter);

            if(result.Length > 0)
            {
                return result;
            }
        }
        return string.Empty;
    }
}

