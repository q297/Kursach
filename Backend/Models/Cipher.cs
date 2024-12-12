namespace Backend;

public class Cipher
{
    private int _columnCount;
    private int _rowCount;
    private string _text;
    private bool _needsRecalculate;
    public string SecretKey { get; set; }

    public int RowCount
    {
        get => _rowCount;
        set
        {
            _rowCount = value;
            _needsRecalculate = true;
        }
    }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            _needsRecalculate = true;
        }
    }
    private void RecalculateColumnCount()
    {
        if (!string.IsNullOrEmpty(_text) && _rowCount > 0)
            _columnCount = (_text.Length + _rowCount - 1) / _rowCount;
        else
            throw new ArgumentException("Введено некорректное значение");

        _needsRecalculate = false;
    }

    public string Encrypt()
    {
        if(_needsRecalculate)
            RecalculateColumnCount();
        char[,] table = new char[RowCount, _columnCount];
        for (int i = 0; i < Text.Length; i++)
        {
            int row = i / _columnCount;
            int col = i % _columnCount;
            table[row, col] = Text[i];
        }
        for (int i = Text.Length; i < RowCount * _columnCount; i++)
        {
            int row = i / _columnCount;
            int col = i % _columnCount;
            table[row, col] = ' ';
        }
        string trimmedKey = SecretKey.Length > _columnCount 
            ? SecretKey.Substring(0, _columnCount) : SecretKey;
        int[] permutation = new int[trimmedKey.Length];

        for (int i = 0; i < permutation.Length; i++)
        {
            permutation[i] = i;
        }
        Array.Sort(trimmedKey.ToCharArray(), permutation);
        char[] result = new char[RowCount * _columnCount];
        int index = 0;
        for (int i = 0; i < _columnCount; i++)
        {
            int permIndex = permutation[i % permutation.Length]; 
            for (int j = 0; j < RowCount; j++)
            {
                result[index++] = table[j, permIndex];
            }
        }

        return new string(result).Trim();
    }

    public string Decrypt()
    {
        int columnCount = (Text.Length + RowCount - 1) / RowCount;

        char[,] table = new char[RowCount, columnCount];

        string trimmedKey = SecretKey.Length > columnCount ? SecretKey.Substring(0, columnCount) : SecretKey;

        int[] permutation = new int[trimmedKey.Length];
        for (int i = 0; i < permutation.Length; i++)
        {
            permutation[i] = i;
        }

        Array.Sort(trimmedKey.ToCharArray(), permutation);

        int index = 0;
        for (int i = 0; i < columnCount; i++)
        {
            int permIndex = permutation[i % permutation.Length];

            for (int j = 0; j < RowCount; j++)
            {
                table[j, permIndex] = Text[index++];
            }
        }

        char[] result = new char[Text.Length];
        index = 0;
        for (int i = 0; i < RowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                result[index++] = table[i, j];
            }
        }

        return new string(result).Trim();
    }
}
 