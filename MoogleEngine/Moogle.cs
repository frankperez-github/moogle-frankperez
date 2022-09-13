namespace MoogleEngine;

public class Moogle
{
    public static SearchResult Query(string query, Dictionary<string, double[]> TF, Dictionary<string, double> iDF, Dictionary<string, Dictionary< string, string>> snippets) {

        // Looking for search operators
        (bool, string[]) nonPresent = operators.nonPresent(query);
        (bool, string[]) Present = operators.Present(query);
        (bool, string[], int) Importance = operators.Importance(query);

        // Proccesing query
        string[] queryWords = preSearch.SplitInWords(query);

        // Texts in Database
        string[] filesAdresses = Directory.GetFiles("../Content/", "*.txt");

        // Array of txt's similarity and adress
        (double, string)[] Match = new (double, string)[filesAdresses.Length];


        // Looking best match in all txt
        for (int i = 0; i < filesAdresses.Length; i++)
        {
            // Search of query
            double queryTF = 0;
            double queryiDF = 0;

            // Creating query vector
            foreach(var word in queryWords)
            {
                // TF of each word in query
                try
                {
                    if(TF[word][i] < 0.02) //0.02 = 1/5, 20% of total of word. If a word appears in less than 20% of total of words, it is not important
                    
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
                    if(iDF[word] > 0.02) //0.02 = 1/5, 20% of total of texts. If a word appears in more than 20% of total of texts, it is not important
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
                    for (int r = 0; r < Importance.Item3; r++) //Number of *
                    {
                        if (TF.ContainsKey(word))
                        {
                            queryTF += TF[word][i];
                            queryiDF += iDF[word];
                        }
                    }
                }
            }

            // Computing SIMILARITY between query and each txt using cotang similarity      
            double similarity = (queryTF / queryiDF);

            // If TF of query in text is 0 discard that txt as match
            if(queryTF == 0)
            {
                Match[i].Item1 = 0; //Score of txt
            }
            else
            {
                Match[i].Item1 = similarity; //Score of txt
            }

            Match[i].Item2 = filesAdresses[i]; //Adress of txt



            // SEARCH OPERATORS
            if (Match[i].Item1 != 0) // Match contains all text in database with its cosine as Item1. Operators works over txts with cosine != 0
            {
                // ! operator
                if (nonPresent.Item1)
                {
                    foreach (var word in nonPresent.Item2)
                    {
                        // If database contains word, discard txts it appears as match
                        if (TF.ContainsKey(word) && TF[word][i] > 0)
                        {
                            Match[i].Item1 = 0; //Similarity is 0
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
                            Match[i].Item1 = 0; //Similarity is 0
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
        
        //In case of not results founded
        if (validMatches == 0)
        {
            SearchItem[] emptySearch = {new SearchItem("No matches founded", "...", 0)};
            return new SearchResult(emptySearch);
        }

        // If there are results to show
        SearchItem[] items = new SearchItem[validMatches];
        int count = 0;

        // Fulling items to be returned
        foreach (var txt in Match)
        {
            // txt.Item2 es adress of txt, 
            // txt.Item1 is score of txt
            double max = double.MinValue;
            string word = "";
            for (int i = 0; i < queryWords.Length; i++)
            {
                if (iDF[queryWords[i]] > max)
                {
                    max = iDF[queryWords[i]];
                    word = queryWords[i];
                }
            }

            // Showing all matches except the ones that have 0 as TF for query
            if(txt.Item1 != 0)
            {
                items[count] = new SearchItem(txt.Item2.Split("../Content/")[1], snippets[word][txt.Item2], txt.Item1);
                count++;
            }   
        }
        
        // Sorting items by Cos(angule)
        var sortedMatches = from item in items orderby item.Score descending select item;
        var results = sortedMatches.ToArray();
            
        return new SearchResult(results, query);
    }

}

    