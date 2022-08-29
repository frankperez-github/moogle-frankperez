namespace MoogleEngine
{
    public class operators
    {
        // operators '!' and '^' return a tuple (bool, string[]) (presence of operator or not, word affected by operator)
        // operator '*' returns (presence of operator or not, word affected by operator, number of '*' in query)
        public static (bool, string[]) nonPresent(string query)
        {
            return operatorAction('!', query);
        }

        public static (bool, string[] affectedWords) Present(string query)
        {
            return operatorAction('^', query);
        }

        public static (bool, string[], int numberOfOperator) Importance(string query)
        {
            (bool, string[], int) Importance = (false, new string[0], 0);
            (bool, string[]) affects = operatorAction('*', query);
            Importance.Item1 = affects.Item1;
            Importance.Item2 = affects.Item2;
            // So far normal operator's behaivor

            // Counting number of * in query
            query = query.Trim();
            int asteriskCount = 0;
            for (int c = 0; c < query.Length; c++)
            {
                if (query[c] == '*')
                {
                    asteriskCount++;
                }
            }
            // Returning total of '*' in query
            Importance.Item3 = asteriskCount;

            return Importance;
        }

        // Aux method
        public static (bool, string[]) operatorAction(char oper, string query)
        {
            // Preparing answers
            List<string> words = new List<string>();
            (bool, string[]) answer = (false, new string[0]);

            query = query.Trim();// Preparing query

            if (query.Contains(oper))
            {   
                // Operator is in query
                answer.Item1 = true;
                
                // Iterating over query looking for operators and words it affects
                for (int c = 0; c < query.Length; c++)
                {
                    if(query[c] == oper)
                    {
                        for (int s = c; s < query.Length; s++)
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
                // returning affected word/s
                answer.Item2 = words.ToArray();
            }

            return answer;
        }
    }

    
}