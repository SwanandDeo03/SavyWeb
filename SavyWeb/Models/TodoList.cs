namespace Project2.Model
{
    public class TodoList
    {
        public object? Title { get; internal set; }
        public object? Description { get; internal set; }
        public bool IsCompleted { get; internal set; }
        public int Id { get; internal set; }
    }
}