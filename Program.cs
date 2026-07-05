using System.Diagnostics;

namespace Cs50Speller;


class Trie
{
    public class Node
    {
        public bool IsWord = false;
        public Node?[] Next = new Node[27];
    }

    public Node?[] Table = new Node[27];
    private uint _wordCount = 0;

    public bool Check(string word)
    {
        //Crawl through each character in the word using the Table
        Node? currentNode = null;
        if (word == "LAND")
        {
            Console.WriteLine("Found it");
        }
        word = word.ToLower();
        
        
        for (int i = 0; i < word.Length; i++)
        {
            //bar
            char character = word[i];
            if (!char.IsAscii(character))
            {
                return false;
            }
            
            int charIndex = character - 'a';
            if (character == '\'')
            {
                charIndex = 26;
            }
            
            if (i == 0)
            {
                currentNode = Table[charIndex];
            }
            else if (i > 0)
            {
                currentNode = currentNode!.Next[charIndex];
            }
            
            if (currentNode == null)
            {
                return false;
            }
            
            if (i == word.Length - 1)
            {
                return currentNode.IsWord;
            }
        }


        return false;
    }
    
    // Loads dictionary into memory, returning true if successful, else false
    public bool Load(string filePath)
    {
        try
        {
            using (StreamReader dictionary = new StreamReader(filePath))
            {
                Node? currentNode = null;
                int i = 0;
                
                while (dictionary.Peek() >= 0)
                {
                    char character = (char)dictionary.Read();
                    if (character == '\n')
                    {
                        currentNode!.IsWord = true;
                        _wordCount++;
                        currentNode = null;
                        i = 0;
                        continue;
                    }
                    
                    int charIndex = character - 'a';
                    if (character == '\'')
                    {
                        charIndex = 26;
                    }
                    
                    if (i == 0)
                    {
                        if (Table[charIndex] == null)
                        {
                            Table[charIndex] = new Node();
                        }
                        
                        currentNode = Table[charIndex];
                        i++;
                    } else if (i > 0)
                    {
                        if (currentNode!.Next[charIndex] == null)
                        {
                            currentNode.Next[charIndex] = new Node();
                        }
                        
                        currentNode = currentNode.Next[charIndex];
                        i++;
                    }
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    // Returns number of words in dictionary if loaded, else 0 if not yet loaded
    public uint Size()
    {
        return _wordCount;
    }

    public bool Unload()
    {
        return true;
    }
}

class Program
{
    static void Main(string[] args)
    {
        // if (args.Length != 3)
        // {
        //     Console.WriteLine("Usage: ./speller [DICTIONARY] [TEXT]");
        //     return;
        // }

        TimeSpan? timeLoad;
        TimeSpan? timeCheck = null;
        TimeSpan? timeSize;
        TimeSpan? timeUnload;

        // Determine dictionary to use
        string reducePath = "../../..";
        string spellerPath = $"{reducePath}/speller";
        string dictionaryPath = $"{spellerPath}/dictionaries/large";
        string textPath =  $"{spellerPath}/texts/lalaland.txt";
        
        Trie trie = new Trie();

        Stopwatch timer = new Stopwatch();
        
        // Load dictionary
        timer.Start();
        bool loaded = trie.Load(dictionaryPath);
        timer.Stop();
        
        // Exit if dictionary not loaded
        if (!loaded)
        {
            Console.WriteLine($"Could not load {dictionaryPath}");
            return;
        }
        
        // Calculate time to load dictionary
        timeLoad = timer.Elapsed;
        
        // Prepare to report misspellings
        Console.WriteLine("\nMISSPELLED WORDS\n");
        
        // Prepare to spell-check
        int index = 0, misspellings = 0, words = 0;
        string word = "";
        
        // Spell-check each word in text
        try
        {
            using (StreamReader textToCheck = new StreamReader(textPath))
            {
                string? line;

                while ((line = textToCheck.ReadLine()) != null)
                {
                    bool keepSkippingAlpha = false;
                    bool keepSkippingAlphaNumeric = false;
                    for (int i = 0; i < line.Length; i++)
                    {
                        char character = line[i];
                        
                        if (keepSkippingAlpha && char.IsAsciiLetter(character))
                        {
                            continue;
                        }
                        
                        if (keepSkippingAlphaNumeric && (char.IsDigit(character) || char.IsAsciiLetter(character)))
                        {
                            continue;
                        }

                        keepSkippingAlpha = false;
                        keepSkippingAlphaNumeric = false;
                        
                        if (char.IsAsciiLetter(character) || (character == '\'' && index > 0))
                        {
                            word += character;
                            index++;

                            if (index > 45)
                            {
                                keepSkippingAlpha = true;
                                index = 0;
                                word = "";
                                continue;
                            }
                            
                            //Check if it's the end of the line
                            if (i == line.Length - 1)
                            {
                                words++;
                            
                                // Check word's spelling
                                timer = Stopwatch.StartNew();
                                bool misspelled = !trie.Check(word);
                                timer.Stop();
                            
                                // Update benchmark
                                if (!timeCheck.HasValue)
                                {
                                    timeCheck = timer.Elapsed;
                                }
                            
                                timeCheck = timeCheck.Value.Add(timer.Elapsed);
                            
                                // Print word if misspelled
                                if (misspelled)
                                {
                                    Console.WriteLine(word);
                                    misspellings++;
                                }
                            
                                // Prepare for next word
                                index = 0;
                                word = "";
                            }
                        }
                        else if (char.IsDigit(character))
                        {
                            keepSkippingAlphaNumeric = true;
                            index = 0;
                            word = "";
                            continue;
                        }
                        else if (index > 0)
                        {
                            words++;
                            
                            // Check word's spelling
                            timer = Stopwatch.StartNew();
                            bool misspelled = !trie.Check(word);
                            timer.Stop();
                            
                            // Update benchmark
                            if (!timeCheck.HasValue)
                            {
                                timeCheck = timer.Elapsed;
                            }
                            
                            timeCheck = timeCheck.Value.Add(timer.Elapsed);
                            
                            // Print word if misspelled
                            if (misspelled)
                            {
                                Console.WriteLine(word);
                                misspellings++;
                            }
                            
                            // Prepare for next word
                            index = 0;
                            word = "";
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }
        
        // Determine dictionary's size
        timer = Stopwatch.StartNew();
        uint n = trie.Size();
        timer.Stop();
        
        // Calculate time to determine dictionary's size
        timeSize = timer.Elapsed;
        
        // Unload dictionary
        timer = Stopwatch.StartNew();
        trie = null;
        timer.Stop();
        
        // Calculate time to unload dictionary
        timeUnload = timer.Elapsed;

        TimeSpan totalTime = timeLoad.Value.Add(timeCheck!.Value).Add(timeSize.Value).Add(timeUnload.Value);
        
        // Report benchmarks
        Console.WriteLine();
        Console.WriteLine($"WORDS MISSPELLED:     {misspellings}");
        Console.WriteLine($"WORDS IN DICTIONARY:  {n}");
        Console.WriteLine($"WORDS IN TEXT:        {words}");
        Console.WriteLine($"TIME IN load:         {timeLoad.Value.Seconds:00}.{(timeLoad.Value.Milliseconds / 10):00}");
        Console.WriteLine($"TIME IN check:        {timeCheck.Value.Seconds:00}.{(timeCheck.Value.Milliseconds / 10):00}");
        Console.WriteLine($"TIME IN size:         {timeSize.Value.Seconds:00}.{(timeSize.Value.Milliseconds / 10):00}");
        Console.WriteLine($"TIME IN unload:       {timeUnload.Value.Seconds:00}.{(timeUnload.Value.Milliseconds / 10):00}");
        Console.WriteLine($"TIME IN TOTAL:        {totalTime.Seconds:00}.{(totalTime.Milliseconds / 10):00}");
        Console.WriteLine();
    }
}