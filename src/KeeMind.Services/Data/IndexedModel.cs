namespace KeeMind.Services.Data;

public class IndexedModel<T>
{
    public IndexedModel(T model, int index)
    {
        Model = model;
        Index = index;
    }

    public T Model { get; }
    public int Index { get; set; }
}
