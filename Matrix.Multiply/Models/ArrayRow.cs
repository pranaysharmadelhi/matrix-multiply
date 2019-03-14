using System;
namespace Matrix.Multiply.Models
{
    public class ArrayRow
    {
        public ArrayRow(int[] _row, ArrayTypes _arrayType, int _rowIndex)
        {
            row = _row;
            arrayType = _arrayType;
            rowIndex = _rowIndex;
        }

        public int[] row { get; set; }
        public ArrayTypes arrayType { get; set; }
        public int rowIndex { get; set; }
    }
}
