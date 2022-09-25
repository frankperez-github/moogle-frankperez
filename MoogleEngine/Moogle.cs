using System.Diagnostics;
namespace MoogleEngine;

public class Moogle
{
    public static SearchResult Query(string query, Dictionary<string, double[]> TF, Dictionary<string, double> iDF, Dictionary<string, string[]> snippets, Dictionary<string, Dictionary<int, int[]>> positionsDict) {

        Stopwatch SearchCrono = new Stopwatch();
        SearchCrono.Start();

        // Looking for search operators
        (bool, string[]) nonPresent = operators.nonPresent(query);
        (bool, string[]) Present = operators.Present(query);
        (bool, string[], int) Importance = operators.Importance(query);
        (bool, Dictionary<string, string[]>) closeness = operators.closeness(query);

        // Proccesing query
        string[] queryWords = preSearch.SplitInWords(query);

        // Texts in Database
        string[] filesAdresses = Directory.GetFiles("../Content/", "*.txt");

        // Array of txt's anguleCos and adress
        (double, string)[] Match = new (double, string)[filesAdresses.Length];

        
        double[] closerInTxt = new double[filesAdresses.Length]; // Final results of calculating distance between words


        // Looking best match in all txt
        for (int i = 0; i < filesAdresses.Length; i++)
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
                    if(TF[word][i] < 0.04) //4% of total of word. If a word appears in more than 4% of total of words, it is not important for that txt
                    {
                        queryTF += TF[word][i];
                    }
                    else
                    {
                        queryTF += 0;
                    }
                }
                catch (KeyNotFoundException)
                {
                    queryTF += 0;
                }

                // iDF of each word in query
                try
                {
                    if(((1.00/(iDF[word])) / filesAdresses.Length) < 0.7) // 70% of total of texts. If a word appears in more than 70% of total of texts, it is not important
                    {
                        queryiDF += iDF[word];
                    }
                    else
                    {
                        queryiDF += 0;
                    }
                }
                catch (KeyNotFoundException)
                {
                    queryiDF += 0;
                }
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

                // ~ operator
                if (closeness.Item1)
                {   
                    // Repeating process for each pair of word affected by operator (repeating for each operator)
                    foreach(var pair in closeness.Item2)
                    {                        
                        // Left word
                        string word = pair.Key;
                        // All words on right associated to left word
                        string[] AffectedWords = pair.Value;

                        // Iterating over each word affected by the word on the left of operator
                        foreach (var affected in AffectedWords)
                        {
                            if (word != affected)
                            {

                                int lWordAppears;
                                int rWordAppears;
                                // Computing distance only if words appears on it
                                if (positionsDict[word].ContainsKey(i) && positionsDict[affected].ContainsKey(i))
                                {
                                    lWordAppears = positionsDict[word][i].Length;
                                    rWordAppears = positionsDict[affected][i].Length;

                                    int minDistance = int.MaxValue;

                                    // Min distance will be 1, if it is already go to next txt
                                    if (minDistance > 1)
                                    {
                                        for (int w = 0; w < lWordAppears; w++)
                                        {
                                            for (int a = 0; a < rWordAppears; a++)
                                            {
                                                int distance = Math.Abs(preSearch.positionsDict[word][i][w] - preSearch.positionsDict[affected][i][a]);
                                                
                                                if (distance < minDistance)
                                                {
                                                    minDistance = distance;
                                                }
                                            }
                                        }
                                        closerInTxt[i] = 1.00 /(minDistance + 1.00);
                                    }
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
            double max = double.MinValue;
            string word = "";

            // Looking for more important word in query(bigger iDF), word most appear in txt
            for (int i = 0; i < queryWords.Length; i++)
            {    
                if (iDF.ContainsKey(queryWords[i]) && iDF[queryWords[i]] > max && TF[queryWords[i]][txtCounter]!= 0)
                {
                    max = iDF[queryWords[i]];
                    word = queryWords[i]; // Most important word will decide which snippet will be showed
                }
            }

            // Showing all matches except the ones that have 0 as TF for query     
            if (match.Item1 != 0 && snippets[word][txtCounter] != null && match.Item2 != null)
            {             
                double score;
                if (closeness.Item1)
                {
                    score = (Math.Truncate(((match.Item1) + closerInTxt[txtCounter]) * 1000) / 1000);
                }
                else
                {
                    score = Math.Truncate(match.Item1*1000) / 1000;
                }

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

        Console.WriteLine("Searched in: "+(double)SearchCrono.ElapsedMilliseconds / 1000+" secs⌚");
        return new SearchResult(results, query);
    }

}

    