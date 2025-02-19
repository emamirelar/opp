namespace UNOPS.PAO.Domain.Infrastructure;

public class DependencyInjectionContainer
{
    private readonly Func<Type, object> getter;

    public DependencyInjectionContainer(Func<Type, object> getter)
    {
        this.getter = getter;
    }

    public object GetInstance(Type type)
    {
        return getter(type);
    }

    public T GetInstance<T>()
    {
        return (T)GetInstance(typeof(T));
    }
}