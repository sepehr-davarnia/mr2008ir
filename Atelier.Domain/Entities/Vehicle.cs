public class Vehicle : Entity
{
    public string Make { get; private set; }
    public string Model { get; private set; }
    public int YearFrom { get; private set; }
    public int? YearTo { get; private set; }
    public string Engine { get; private set; }
    public string Trim { get; private set; }
    public string Slug { get; private set; }
    public bool IsActive { get; private set; }

    protected Vehicle()
    {
        Make = Model = Engine = Trim = Slug = "N/A";
    }

    public Vehicle(string make, string model, int yearFrom, int? yearTo, string engine, string trim, string slug)
    {
        Update(make, model, yearFrom, yearTo, engine, trim);
        Slug = slug;
        IsActive = true;
    }

    public void Update(string make, string model, int yearFrom, int? yearTo, string engine, string trim)
    {
        if (yearFrom is < 1900 or > 2200 || yearTo < yearFrom) throw new ArgumentOutOfRangeException(nameof(yearFrom));
        Make = make.Trim();
        Model = model.Trim();
        YearFrom = yearFrom;
        YearTo = yearTo;
        Engine = engine.Trim();
        Trim = trim.Trim();
        MarkUpdated();
    }

    public void SetActive(bool active) { IsActive = active; MarkUpdated(); }
}
