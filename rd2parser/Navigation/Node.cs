namespace rd2parser.Navigation;
public abstract class Node
{
    public int ReadOffset; // For debugging, reader offset where the object starts when read
    public int WriteOffset; // For debugging, writer offset where the object starts when written
    public int ReadLength; // For debugging, length of the object when read
    public int WriteLength; // For debugging, length of the object when written
    public abstract IEnumerable<Node> GetChildren();
}
