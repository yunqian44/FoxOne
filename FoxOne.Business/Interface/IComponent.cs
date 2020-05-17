namespace FoxOne.Business
{
    public interface ISortableControl : IControl
    {
        int Rank { get; set; }
    }

    public interface IComponent : ISortableControl
    {
        string CssClass { get; set; }

        bool Visiable { get; set; }

        string Render();
    }

    public interface IListDataSourceControl
    {
        IListDataSource DataSource { get; set; }
    }

    public interface ICascadeDataSourceControl
    {
        ICascadeDataSource DataSource { get; set; }
    }

    public interface IPageService
    {

    }


}
