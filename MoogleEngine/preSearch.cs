using System.Diagnostics; // to use a crono

public class preSearch
{   
   
    public static Dictionary<string,string> StringContent = new Dictionary<string, string>(); // Dict with each path text and its contain as string


    // Auxiliar methods
    public static Dictionary<string, string[]> LoadTexts()
    {
        // Addresses of txts (GetFiles returns a string[])
        string[] filesAdresses = Directory.GetFiles("../Content/", "*.txt");
        int TxtQuant = filesAdresses.Length; //Quantity of txts

        // Dictionary to storage all content of txts, each text as an array of "words"
        // and as key the file address
        Dictionary<string, string[]> TXTcontent = new Dictionary<string, string[]>();
        
        // For all text in database
        for (int i = 0; i < TxtQuant; i++)
        {   
            // Reading each line
            string content = File.ReadAllText(filesAdresses[i]);
            
            // Storaging text as string
            if(StringContent.Count != TxtQuant)
            {
                StringContent.Add(filesAdresses[i], content);
            }

            content = content.ToLower();
            
            // Spliting each line in words
            TXTcontent.Add(filesAdresses[i], SplitInWords(content));
        }      
        return TXTcontent;
    } 
 
    public static string[] SplitInWords(string sentence)
    {
        // Normalizing text
        sentence.Trim();
        sentence = sentence.ToLower();
        char[] separators = { ' ', ',', '.', ';', ':','-','+','=','_','%','@','#','$','—','¿','?','(',')','!','¡','«','»','[',']','{','}','^','~','*'};
        string[] words = sentence.Split(separators,StringSplitOptions.RemoveEmptyEntries);
        
        return words;
    }


    // Principal Methods
    public static Dictionary<string, double[]> TF()
    // This method compute TF of all words in all texts and storage it in a dict  <word, TF values[]> pairs 
    {
        // Here I will storage TF for all words
        // In a dict that contains all words and their TF value for each text
        Dictionary<string, double[]> TF = new Dictionary<string, double[]>();

        // Loading all txts to a dict
        // key: textAdress, value: words in text
        Dictionary<string, string[] > TXTsContent = LoadTexts();
        // Quantity of texts
        int totalTXTs = TXTsContent.Count();
        // List of texts' paths
        string[] filesAdresses = Directory.GetFiles("../Content/", "*.txt");

        // Delay time of TF
        Stopwatch crono = new Stopwatch();
        crono.Start();

        // For each text computing TF to words
        for (int t = 0; t < totalTXTs; t++)
        {
            // Loading array of words of acual txt
            string[] actualWords = TXTsContent[filesAdresses[t]];
            
            // Fulling TF dict
            for (int i = 0; i < actualWords.Length; i++)
            {   
                // TF for actual word in all texts
                double[] TFs = new double[filesAdresses.Length];

                // If word already exists, just add (1 / length of doc.) to TF, else add it to dict
                if (TF.ContainsKey(actualWords[i].ToLower()))
                {
                    TF[actualWords[i]][t] += (double)(1.00 / (double)actualWords.Length); 
                }
                else
                {
                    TF.Add(actualWords[i].ToLower(), TFs);
                    TF[actualWords[i]][t] += (double)(1.00 / (double)actualWords.Length);
                }
            }

        }   
        
        
        Console.WriteLine("TF Finished in: "+(double)crono.ElapsedMilliseconds/1000+" secs.⌚");
        crono.Stop();

        return TF;
    }
        
    public static Dictionary<string, double> iDF(Dictionary<string, double[]> TF)
    // This method compute iDF of all words in database and storage it in a dict  <word, iDF value> pairs
    {
        // Dictionary to storage iDF value of each word
        Dictionary<string, double> iDF = new Dictionary<string, double>();

        // Loading all txts to a dict
        // key: textAdress, value: words in text
        Dictionary<string, string[] > TXTsContent = LoadTexts();
        int totalTXTs = TXTsContent.Count();

        // Delay time of iDF
        Stopwatch crono = new Stopwatch();
        crono.Start();

        // For each word in TF dict if it has a TF value, that means it appears in that txt
        foreach(var tf in TF)
        {
            // Storing TF values array of each word in TF dictionary
            double[] tfArray = tf.Value;
            
            // For each text, if word have a TF value for this txt, add 1 to iDF
            for (int j = 0; j < tfArray.Length; j++)
            {
                if (tfArray[j] != 0)
                {
                    if (iDF.ContainsKey(tf.Key.ToLower()))
                    {
                        iDF[tf.Key]++;
                    }
                    else
                    {
                        iDF.Add(tf.Key.ToLower(), 1);
                    }
                }
            } 
        }

        foreach(var idf in iDF)
        {
            // iDf value is: ( 1 / frequency of a word in data base ) frequency in database is never going to be 0 because is calculated based on TF dictionary of all words
            // A lower value of iDF represents a very common word in database
            iDF[idf.Key] = (1 / iDF[idf.Key]);
        }
        

        Console.WriteLine("iDF Finished in: "+(double)crono.ElapsedMilliseconds/1000+" secs.⌚");
        crono.Stop();

        return iDF;   
    }

    public static Dictionary<string, Dictionary<string, string>> snippets(Dictionary<string, double[]> TF)
    {
        Dictionary<string, string[]> texts = LoadTexts();

        //          word            path of txt, snippet 
        Dictionary<string, Dictionary< string, string>> snippets = new Dictionary<string, Dictionary<string, string>>();
        
        Stopwatch crono = new Stopwatch();
        crono.Start();

        foreach (var word in TF)
        {
            foreach (var text in texts)
            {

                string snippet = "";
                string txtContain = StringContent[text.Key];

                
                if(txtContain.Contains(word.Key) && txtContain.Length > 10)
                {
                    // snippet = txtContain.Substring(index, length); 
                }
                
                // Copying snippet of each word to snippet Dict, creating word if it doesn't exist or updating snippets of words in tetxs
                if (snippets.ContainsKey(word.Key))
                {
                    snippets[word.Key].Add(text.Key, snippet);
                }
                else
                {
                    Dictionary<string, string> textSnippet = new Dictionary<string, string>();
                    textSnippet.Add(text.Key, snippet);
                    
                    snippets.Add(word.Key, textSnippet);
                }
            }
        }
        
        Console.WriteLine("Snippets loaded in: "+crono.ElapsedMilliseconds/1000+" secs.⌚");
        crono.Stop();
        return snippets;
    }
}
