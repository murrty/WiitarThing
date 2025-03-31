namespace WiitarThing;

internal struct PointD {
    [System.Xml.Serialization.XmlAttribute]
    public double X;

    [System.Xml.Serialization.XmlAttribute]
    public double Y;

    public PointD(double x, double y)
    {
        X = x;
        Y = y;
    }
}
