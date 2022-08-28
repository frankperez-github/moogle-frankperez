namespace MoogleEngine;

public class Moogle
{
    public static SearchResult Query(string query, Dictionary<string, double[]> TF, Dictionary<string, double> iDF) {

        // Looking for search operators
        (bool, string[]) nonPresent = operators.nonPresent(query);
        (bool, string[]) Present = operators.Present(query);


        // Proccesing query
        string[] queryWords = preSearch.SplitInWords(query);

        // Texts in Database
        string[] filesAdresses = Directory.GetFiles("../Content/", "*.txt");

        // Array of txt's similarity and adress
        (double, string)[] Match = new (double, string)[filesAdresses.Length];


        // Real matches
        int validMatches = 0;

        // Looking best match in all txt
        for(int i = 0; i < filesAdresses.Length; i++)
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
                    queryTF += TF[word][i];
                }
                catch (KeyNotFoundException)
                {
                    queryTF += 0;
                }

                // iDF of each word in query
                try
                {
                    queryiDF += iDF[word];
                }
                catch (KeyNotFoundException)
                {
                    queryiDF += 0;
                }
            }

            // Computing SIMILARITY between query and each txt using cosine similarity
            // The bigger Cos(angule) is best match            
            double TXTvectorLength = Math.Sqrt(Math.Pow(queryTF, 2) + Math.Pow(queryiDF, 2)); //Length of vector of txt
            double anguleCos = (queryTF / TXTvectorLength);

            // If TF of query in text is 0 discard that txt as match
            if(queryTF == 0)
            {
                Match[i].Item1 = 0;   //Score of txt
            }
            else
            {
                Match[i].Item1 = anguleCos; //Score of txt
                validMatches++;
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
                            validMatches--; //Previously this text was a match, now it's discarded
                            break; //Text is discarded only one time. If a word of affected ones is founded in text, TXT will be discarded
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
                            validMatches--; //Previously this text was a match, now it's discarded
                            break; //Text is discarded only one time. If a word of affected ones is not founded in text, TXT will be discarded
                            
                        }
                    }
                }
            }
        }
            
        

        // Results of search are just valid matches
        SearchItem[] items = new SearchItem[validMatches];
        int count = 0;
        
        //In case of not results founded
        if (validMatches == 0)
        {
            SearchItem[] emptySearch = {new SearchItem("No matches founded", "...", 0)};
            return new SearchResult(emptySearch);
        }

        // Fulling items to be returned
        foreach (var txt in Match)
        {
            // txt.Item1 es adress of txt, 
            // txt.Item2 is score of txt

            // Showing all matches except the ones that have 0 as TF for query
            if(txt.Item1 != 0)
            {
                items[count] = new SearchItem(txt.Item2.Split("../Content/")[1], "Lorem ipsum dolor sit amet", txt.Item1);
                count++;
            }   
        }
        
        // Sorting items by Cos(angule)
        var sortedMatches = from item in items orderby item.Score descending select item;
        var results = sortedMatches.ToArray();
            
        return new SearchResult(results, query);
    }

}

    