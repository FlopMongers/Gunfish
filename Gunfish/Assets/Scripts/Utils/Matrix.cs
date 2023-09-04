using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matrix
{
    public float[,] matrix;
    public int width { get; private set; }
    public int height { get; private set; }

    public Matrix(int width, int height) {
        this.width = width;
        this.height = height;
    }

    public Matrix Multiply(Matrix m) {
        return new Matrix(width, height);
    }
}
