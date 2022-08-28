namespace MoogleEngine
{
    public class operators
    {
        public static (bool, string[]) nonPresent(string query)
        {
            query = query.Trim();
            (bool, string[]) nonPresent = (false, new string[0]);


            // If operator is present in query, find wich word/s is modifying
            if(query.Contains('!'))
            {
                // Operator is in query so Item1 = true
                nonPresent.Item1 = true;
                // Finding affected word/s and returning it/them
                string[] affectedWords = OperatorAffects('!', query);
                nonPresent.Item2 = affectedWords;
            }

            return nonPresent;
        }

        public static (bool, string[]) Present(string query)
        {
            query = query.Trim();
            (bool, string[]) Present = (false, new string[0]);

            if (query.Contains('^'))
            {   
                // Operator is in query
                Present.Item1 = true;
                // Finding affected word/s and returning it/them
                string[] affectedWords = OperatorAffects('^', query);
                Present.Item2 = affectedWords;
            }

            return Present;
        }

        // Aux method
        public static string[] OperatorAffects(char oper, string query)
        {
            List<string> words = new List<string>();

            // Iterating over query looking for operators and words it affects
            for(int c = 0; c < query.Length; c++)
            {
                if(query[c] == oper)
                {
                    for(int s = c; s < query.Length; s++)
                    {
                        if(query[s] == ' ' || s == query.Length-1)
                        {
                            string word = query.Substring(c+1, s-c); // Affected word starts after operators and ends on next white space or end of query 
                            word = word.Trim();
                            words.Add(word); // Agregate word to the array would be returned
                            break; // Go out loop and look for new operator and new word
                        }
                    }
                }
            }
            return words.ToArray();
        }
    }

    
}