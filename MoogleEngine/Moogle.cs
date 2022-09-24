namespace MoogleEngine;
using System.Text.Json;
public class Moogle
{
    public static SearchResult Query(string query, Dictionary<string, double[]> TF, Dictionary<string, double> iDF, Dictionary<string, string[]> snippets) {

        // Looking for search operators
        (bool, string[]) nonPresent = operators.nonPresent(query);
        (bool, string[]) Present = operators.Present(query);
        (bool, string[], int) Importance = operators.Importance(query);
        (bool, Dictionary<string, string[]>) closeness = operators.closeness(query);

        // Proccesing query
        string[] queryWords = preSearch.SplitInWords(query);

        // Texts in Database
        string[] filesAdresses = Directory.GetFiles("../Content/", "*.txt");   
        Dictionary<string, string[]> texts = preSearch.LoadTexts();


        // Array of txt's anguleCos and address
        (double, string)[] Match = new (double, string)[texts.Count()];

        int[] closenessInTxt = new int[texts.Count()]; // Final results of calculating distance between words


        // Looking best match in all txt
        for (int i = 0; i < texts.Count; i++)
        {

            // Search of query
            double queryTF = 0;
            double queryiDF = 0;

            // Creating query vector of each txt
            foreach(var word in queryWords)
            {
                // TF of each word in query
                try
                {
                    if(TF[word][i] < 0.03) //3% of total of word. If a word appears in more than 4% of total of words, it is not important for that txt
                    {
                        queryTF += TF[word][i];
                    }
                    else
                    {
                        queryTF += 0;
                    }
                }
                catch (KeyNotFoundException)
                {}

                // iDF of each word in query
                try
                {
                    if(((1.00/(iDF[word])) / filesAdresses.Length) < 0.7) // 70% of total of texts. If a word appears in more than 70% of total of texts, it is not important
                    {
                        queryiDF += iDF[word];
                    }
                    else
                    {
                        iDF[word] = 0;
                    }
                }
                catch (KeyNotFoundException)
                {}
            }

            // * operator
            if (Importance.Item1)
            {
                // Each word affected by * operator, will be multyplied by number of repetitions of * in query
                foreach (var word in Importance.Item2)
                {
                    if (TF.ContainsKey(word))
                    {
                                              // Total of asterisks
                        queryTF += TF[word][i] * Importance.Item3; 
                        queryiDF += iDF[word] * Importance.Item3;
                    }
                }
            }


            // Computing SIMILARITY between query and each txt using cosine similarity  
            double vectorLength = Math.Sqrt(Math.Pow(queryTF, 2) + Math.Pow(queryiDF, 2));
            double anguleCos =  (queryTF + queryiDF) / (Math.Sqrt(2) * vectorLength);
            

            // If TF of query in text is 0 discard that txt as match
            if(queryTF == 0)
            {
                Match[i].Item1 = 0; //Score of txt
            }
            else
            {
                Match[i].Item1 = anguleCos; //Score of txt
            }
            Match[i].Item2 = filesAdresses[i]; //Adress of txt


            // SEARCH OPERATORS
            if (Match[i].Item1 != 0) // Match contains all text in database with its similarity as Item1. Operators works over txts with cosine != 0
            {
                // ! operator
                if (nonPresent.Item1)
                {
                    foreach (var word in nonPresent.Item2)
                    {
                        // If database contains word, discard txts it appears as match
                        if (TF.ContainsKey(word) && TF[word][i] > 0)
                        {
                            Match[i].Item1 = 0; //anguleCos is 0
                        }
                    }
                }

                // ^ operator
                if (Present.Item1)
                {
                    foreach (var word in Present.Item2)
                    {
                        // If database contains word, discard txts it doesn't appear
                        if (TF.ContainsKey(word) && TF[word][i] == 0)
                        {
                            Match[i].Item1 = 0; //anguleCos is 0
                        }
                    }
                }

                StreamReader text = new StreamReader(filesAdresses[i]);
                // ~ operator
                if (closeness.Item1)
                {
                    foreach(var pair in closeness.Item2)
                    {
                        string word = pair.Key;
                        string[] AffectedWords = pair.Value;
                        foreach (var affected in AffectedWords)
                        {
                            if (word != affected)
                            {
                                string[] textWords = texts[filesAdresses[i]];
                                int minDistance = int.MaxValue;

                                // Looking for words in text to calculate distance
                                if (iDF.ContainsKey(word) && iDF.ContainsKey(affected) && TF[word][i] != 0 && TF[affected][i] != 0 && minDistance > 1)
                                {
                                    Console.WriteLine(i);
                                    for (int f = 0; f < textWords.Length; f++)
                                    {
                                        if (textWords[f].ToLower() == word)
                                        {
                                            for (int r = 0; r < textWords.Length; r++)
                                            {
                                                if (textWords[r].ToLower() == affected)
                                                {
                                                    if (Math.Abs(f - r) < minDistance)
                                                    {
                                                        minDistance = Math.Abs(f - r);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    // Min distance can be 0, in that case I will sum 0.5 to score
                                    Match[i].Item1 += 0.1;
                                }
                            } 
                        }
                    }
                }
            }
        }        

        // Results of search are just valid matches (score != 0)
        int validMatches = 0;
        foreach (var match in Match)
        {
            if (match.Item1 != 0)
            {
                validMatches++;
            }
        }
        

        // If there are results to show
        SearchItem[] items = new SearchItem[validMatches];
        int count = 0;

        // Fulling items to be returned
        int txtCounter = 0;
        foreach (var match in Match)
        {
            // txt.Item2 es adress of txt, 
            // txt.Item1 is score of txt
            double max = double.MaxValue;
            string word = "";

            // Looking for more important word in query(bigger iDF), word most appear in txt
            for (int i = 0; i < queryWords.Length; i++)
            { 
                if (TF.ContainsKey(queryWords[i]) && iDF[queryWords[i]] < max && TF[queryWords[i]][txtCounter]!= 0)
                {
                    max = iDF[queryWords[i]];
                    word = queryWords[i]; // Most important word will decide which snippet will be showed
                }
            }

            // Showing all matches except the ones that have 0 as TF for query     
            if (match.Item1 != 0 && snippets[word][txtCounter] != null)
            {             
                double score;
                
                score = Math.Truncate(match.Item1*1000) / 1000;
                items[count] = new SearchItem(snippets[word][txtCounter], score, match.Item2);

                count++;
            }  
            txtCounter++; 

            // If there are few results maybe query was wrong
            // Looking for best word in database to suggest using Hammming distance
            if (validMatches < 5)
            {
                string[] newQuery = new string[queryWords.Length];
                int wordCounter = 0;

                foreach (string Word in queryWords)
                {
                    int minDif = int.MaxValue;
                    string bestSug = "";

                    // Looking for most similar word in database (words in TF)
                    foreach (var posibleSug in TF)
                    {
                        int diff = 0;
                        int minLength = Math.Min(Word.Length, posibleSug.Key.Length);

                        // Comparing each character of words
                        for (int c = 0; c < minLength; c++)
                        {
                            if (Word[c] != posibleSug.Key[c])
                            {
                                diff++;
                            }
                        }
                        // Adding to diff, difference of length of both words
                        diff += Math.Max(Word.Length, posibleSug.Key.Length) - Math.Min(Word.Length, posibleSug.Key.Length);
                        
                        if (diff < minDif)
                        {
                            minDif = diff;
                            bestSug = posibleSug.Key;
                        }
                    }
                    newQuery[wordCounter] = bestSug;
                    wordCounter++;
                }
                query = string.Join(' ', newQuery);
            }

            //In case of not results founded
            if (validMatches == 0)
            {
                SearchItem[] emptyResult = {new SearchItem("No matches", 0, "#")};
                return new SearchResult(emptyResult, query);
            }
        }
        
        // Sorting items by Cos(angule)
        var sortedMatches = from item in items orderby item.Score descending select item;
        var results = sortedMatches.ToArray();

        // If there are so much results showing best 10
        if (results.Count() > 10)
        {
            SearchItem[] finalresults = new SearchItem[10];

            for(int counter = 0; counter < 10; counter++)
            {
                finalresults[counter] = results[counter];
            }
            return new SearchResult(finalresults,query);
        }
            
        return new SearchResult(results, query);
    }

}

    