namespace MoogleEngine;

public class SearchItem
{
    public SearchItem(string snippet, double score, string address)
    {
        this.Snippet = snippet;
        this.Score = score;
        this.Address = address;
    }

    public string Title { get; private set; }
    
    public string Address { get; private set; }

    public string Snippet { get; private set; }

    public double Score { get; private set; }
}
