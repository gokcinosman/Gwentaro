public interface IObserver
{
    void OnNotify(string message, BoardRow boardRow);
}