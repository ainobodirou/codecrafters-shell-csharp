using System;
using System.Collections.Generic;

class Trie
{
    private class TrieNode
    {
         public TrieNode[] children = new TrieNode[26];
         public bool isWordEnd = false;
    }
    TrieNode root;
    public Trie() {
        root = new TrieNode();
    }

    public void insert(string key)
    {
        TrieNode pCrawl = root;
        for (int i = 0; i < key.Length; i++)
        {
            int index = key[i] - 'a';
            if (pCrawl.children[index] == null)
            {
                pCrawl.children[index] = new TrieNode();
            }
            pCrawl = pCrawl.children[index];
            // Mark last node as leaf
        }
        pCrawl.isWordEnd = true;
    }

    public string GetCompletion(string query)
    {
        if (string.IsNullOrEmpty(query))
            {
                return "";
            }
        // autocomplete the typed command
        TrieNode pCrawl = root;
        for(int i = 0; i <query.Length; i++)
        {
            
            int index = query[i] - 'a';
            if(index < 0 || index >= 26)
            {
                return "";
            }
            if (pCrawl.children[index] == null)
            {
                return "";
            }
            pCrawl = pCrawl.children[index];
        }
        return AutoComplete(pCrawl, query);
    }
        string AutoComplete(TrieNode root, string currPrefix) {
        // found a string in Trie with the given prefix
        if (root.isWordEnd) {
            return currPrefix;
        }
        for (int i = 0; i < 26; i++) {
            if (root.children[i] != null) {
                // child node character value
                char c = (char)(i + 'a');
                return AutoComplete(root.children[i], currPrefix + c);
            }
        }
        return "";
    }
}

