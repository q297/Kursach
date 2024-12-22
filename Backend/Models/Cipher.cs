internal class Crypter
{
    private string _keyword;
    private int[] _permutation;
    private int _width;
    public int Height;

    public string Keyword
    {
        get => _keyword;
        set
        {
            _keyword = value;
            _width = _keyword.Length;
            _permutation = new int[_width];
            for (var i = 0; i < _width; i++) _permutation[i] = i;

            Array.Sort(_keyword.ToCharArray(), _permutation);
        }
    }

    public string Encode(string src)
    {
        var totalCells = _width * Height;
        
        if (src.Length < totalCells)
        {
            src = src.PadRight(totalCells, ' ');
        }
        else if (src.Length > totalCells)
        {
            var newHeight = (int)Math.Ceiling((double)src.Length / _width);
            Height = newHeight;
            totalCells = _width * Height;
            src = src.PadRight(totalCells, ' ');
        }

        var res = new char[src.Length];
        for (var i = 0; i < Height; i++)
        for (var j = 0; j < _width; j++)
            res[i * _width + j] = src[_permutation[j] * Height + i];
        return new string(res);
    }

    public string Decode(string src)
    {
        var totalCells = _width * Height;
        
        if (src.Length < totalCells)
        {
            src = src.PadRight(totalCells, ' ');
        }
        else if (src.Length > totalCells)
        {
            var newHeight = (int)Math.Ceiling((double)src.Length / _width);
            Height = newHeight;
            totalCells = _width * Height;
            src = src.PadRight(totalCells, ' ');
        }

        var res = new char[src.Length];
        for (var i = 0; i < _width; i++)
        for (var j = 0; j < Height; j++)
            res[_permutation[i] * Height + j] = src[j * _width + i];
        return new string(res).TrimEnd();
    }
}