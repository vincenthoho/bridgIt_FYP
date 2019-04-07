using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class shannon : MonoBehaviour {
	private static float[] res = new float[41];	//array carrying the resistant of each edge [index<size*size -> horizontal] [index >= size*size -> vertical]
	private static float[] current = new float[41]; //array for cureent in each edge [index<size*size -> horizontal] [index >= size*size -> vertical]
	private static ArrayList record = new ArrayList();//arraylist for checking edge already checked for connectEdge()

	// Use this for initialization
	void Awake () {
		//set unity resistance for all edge
		for (int i = 0; i < 41; i++) {
			res[i] = 1;
		}
	}

	//Reset the static variable
	public static void reset(){
		for (int i = 0; i < 41; i++) {
			res [i] = 1;
			current [i] = 0;
		}
		record.Clear ();
	}

	//Set resistance
	//Set as 1e-08 when computer connect -> (float)decimal.Parse("1E-08", NumberStyles.Float)
	//Set as 0 when player cut
	public static void setResistance(int i, float r){
		res[i] = r;
	}
		
	private static void calCurrent(int nx){
		//empty the current array
		for (int x = 0; x < current.Length; x++)
			current [x] = 0;
		
		//nx -> board size
		int nloops = nx*(nx - 1) + 1;	//number of loops
		for (int x = 0; x < res.Length; x++) {
			if (res [x] == -1) {
				res [x] = 0;
			}
		}

		double[][] mt = new double[nloops][];
		for (int x = 0; x < nloops; x++)
			mt [x] = new double[nloops + 1];
		mt[0][nloops] = 1;	//set first row's solution be 100 -> the flowing voltage be 1

		//Fill the matrix up
		int i = 0;
		float rloop = 0;
		while(i < nx){
			int iup = i*nx;
			int iadj = i*(nx-1)+1;
			float rloc = res[iup];
			rloop += rloc;


			mt[0][iadj] = -rloc;
			mt[iadj][0] = -rloc;
			mt[iadj][iadj] = mt[iadj][iadj]+rloc;
			i++;
		}
		mt[0][0] = rloop;

		int j = 0;
		int iloop = 0;
		while(j < nx-1){
			i = 0;
			while(i < nx){
				int iup = i*nx+j+1;
				int ithis = i*(nx-1)+j+1;
				int iadj = i*(nx-1)+j+2;
				float rloc = res[iup];
				mt[ithis][ithis] = mt[ithis][ithis]+rloc;
				if(j < nx-2){
					
					mt[iadj][iadj] = mt[iadj][iadj]+rloc;
					mt[ithis][iadj] = -rloc;
					mt[iadj][ithis] = -rloc;
				}
				if(i<nx-1){
					iup = nx*nx+i*(nx-1)+j;
					iadj = ithis+(nx-1);
					rloc = res[iup];

					mt[ithis][ithis] = mt[ithis][ithis]+rloc;
					mt [iadj][iadj] = mt [iadj][iadj] + rloc;
					mt [ithis][iadj] = -rloc;
					mt [iadj][ithis] = -rloc;
				}
				i++; iloop++;
			}
			j++;
		}

		//Solve the matrix with Gaussian Elimination
		double[] vc = SolveLinearEquations(mt);

		//Get the voltage of each node with the solved matrix
		int[] volt = new int[nloops];
		for (i = 0; i < nloops; i++) {
			volt [i] = (int)(300 *vc[i]);
		}

		//Set voltage of node connected be the same
		for (i = 0; i < nx * nx + (nx - 1) * (nx - 1); i++) {
			if (res [i] != 0 && res [i] != 1) {
				if (i < nx * nx) {		//horizontal edge
					int ix = i / nx;	//row index
					int iy = i % nx;	//col index

					int iup = 0;		//node at left of current edge
					if (iy != 0)
						iup = i - ix;

					int idn = 0;		//node at right of current edge
					if (iy > nx - 2)
						idn = -1;
					else
						idn = i - ix + 1;

					if (idn > 0) {
						volt [idn] = volt [iup];
					}
				} else {	//vertical edge
					j = i - nx * nx;		//vertical index
					int ix = j / (nx - 1);	//row index
					int iy = j % (nx - 1);	//col index
					int iup = ix * (nx - 1) + iy + 1;	//node at up of current edge
					int idn = iup + (nx - 1);	//node at down of current edge
					volt[idn] = volt[iup];
				}
			}
		}

		//Set voltage be 0 if edge connected to the right-most node
		for (int x = 0; x < nx; x++) {
			int y = (nx-1) + nx*x;
			if(res[y] != 0 && res[y] != 1)
				volt = connectEdge (y, nx, volt);
		}
		record.Clear ();

		//Calculate the current of each edge
		for (i = 0; i < nx*nx+(nx-1)*(nx-1); i++) {
			if (res [i] == 1) { 		//not being cut or short-circuit
				if (i < nx * nx) {		//horizontal edge
					int ix = i / nx;	//row index
					int iy = i % nx;	//col index

					int iup = 0;		//node at left of current edge
					if (iy != 0)
						iup = i - ix;

					int idn = 0;		//node at right of current edge
					if (iy > nx - 2)
						idn = -1;
					else
						idn = i - ix + 1;

					if (idn > 0)
						current [i] = volt [iup] - volt [idn];
					else
						current [i] = volt [iup];
				} else {	//vertical edge
					j = i - nx * nx;		//vertical index
					int ix = j / (nx - 1);	//row index
					int iy = j % (nx - 1);	//col index
					int iup = ix * (nx - 1) + iy + 1;	//node at up of current edge
					int idn = iup + (nx - 1);	//node at down of current edge
					current [i] = volt [iup] - volt [idn];
				}
			}
		}

		//For debug
		string msg = "";
		for(int x = 0; x < nloops; x++){
			msg = "<color=green>";
			for (int y = 0; y < nloops+1; y++) {
				msg += mt [x][y];
				if (y == nloops) {
					msg += "</color>";
				}else
					msg += ", ";
			}
			Debug.Log (msg);
		}
		for (int x = 0; x < nloops; x++) {
			//Debug.Log ("<color=red>" + (int)(vc [x]) + "</color>");
			//Debug.Log ("<color=red>"+(int)(300*vc[x])+"</color>");
			Debug.Log ("<color=red>"+(int)(volt[x])+"</color>");
			//Debug.Log ("<color=red>"+(int)(mt[x, nloops])+"</color>");
		}
		for (int x = 0; x < nx*nx+(nx-1)*(nx-1); x++) {
			Debug.Log ("<color=blue>"+current[x]+"</color>");
		}
	}
		
	//nx -> board size
	public static int getMaxCurrent(int nx){
		calCurrent (nx);

		float maxCur = -1;
		int maxIndex = 0;
		List<int> maxCurs = new List<int> ();

		//compare current and record largest current to array
		for (int i = 0; i < nx * nx + (nx - 1) * (nx - 1); i++) {
			if (current [i] > maxCur) {
				maxCur = current [i];
				maxIndex = i;
				maxCurs.Clear ();
				maxCurs.Add (i);
			}
			else if (current [i] == maxCur) {
				maxCurs.Add (i);
			}
		}

		//if more than one edge with largest current, choose randomly
		if (maxCurs.Count > 1) {
			maxIndex = maxCurs [Random.Range (0, maxCurs.Count)];
			Debug.Log ("<color=purple>random "+maxIndex+"</color>");
			if(maxCur != 0)
				return maxIndex;
		}

		//if largest current found is 0, check negative current also
		if (maxCur == 0) {
			for (int i = 0; i < nx * nx + (nx - 1) * (nx - 1); i++) {
				if (current [i] < maxCur) {
					maxCur = current [i];
					maxIndex = i;
					maxCurs.Clear ();
					maxCurs.Add (i);
				}
				else if (current [i] == maxCur) {
					maxCurs.Add (i);
				}
			}
			if (maxCurs.Count > 1) {
				maxIndex = maxCurs [Random.Range (0, maxCurs.Count)];
				Debug.Log ("<color=purple>random "+maxIndex+"</color>");
				return maxIndex;
			}
		}

		Debug.Log ("<color=purple>"+maxIndex+"</color>");
		return maxIndex;
	}

	//Find edge connected from the end and turn their voltage to 0
	private static int[] connectEdge(int i, int nx, int[] volt){
		int tmpI = i, iup = 0, idn = 0;
		if (i < nx * nx) {
			//check upward
			tmpI = i + (nx * nx - (nx + 1)) - (i/nx - 1);
			if (!record.Contains(tmpI) && tmpI > 0 && tmpI >= nx*nx && tmpI < 41 && res [tmpI] != 0 && res [tmpI] != 1) {
				int ix = (tmpI - (nx * nx)) / (nx - 1);
				int iy = (tmpI - (nx * nx)) % (nx - 1);
				iup = ix * (nx - 1) + iy + 1;
				volt [iup] = 0;
				record.Add (tmpI);
				Debug.Log ("Changed volt " + iup + " to 0");
				volt = connectEdge (tmpI, nx, volt);
			}

			//check downward
			tmpI = i + (nx * nx) - 2 - (i/nx - 1);
			if (!record.Contains(tmpI) && tmpI < (nx * nx + (nx - 1) * (nx - 1)) && tmpI >= nx*nx && res [tmpI] != 0 && res [tmpI] != 1) {
				int ix = (tmpI - (nx * nx)) / (nx - 1);
				int iy = (tmpI - (nx * nx)) % (nx - 1);
				iup = ix * (nx - 1) + iy + 1;
				idn = iup + (nx - 1);
				volt [idn] = 0;
				record.Add (tmpI);
				Debug.Log ("Changed volt " + iup + " to 0");
				volt = connectEdge (tmpI, nx, volt);
			}

			//check left
			tmpI = i-1;
			if (!record.Contains(tmpI) && tmpI % nx != 0 && res [tmpI] != 0 && res [tmpI] != 1) {
				int ix = tmpI / nx;
				int iy = tmpI % nx;
				if (iy != 0)
					iup = tmpI - ix;
				volt [iup] = 0;
				record.Add (tmpI);
				Debug.Log ("Changed volt " + iup + " to 0");
				volt = connectEdge (tmpI, nx, volt);
			}
			return volt;
		} else {
			tmpI = i - nx * nx;
			int ix = tmpI / (nx - 1);
			int iy = tmpI % (nx - 1);
			iup = ix * (nx - 1) + iy + 1;
			idn = iup + (nx - 1);

			//check up-left edge
			tmpI = (nx*ix)+iy;
			if (!record.Contains(tmpI) && tmpI % nx > 0 && res [tmpI] != 0 && res [tmpI] != 1) {
				volt [iup - 1] = 0;
				record.Add (tmpI);
				Debug.Log ("Changed volt " + (iup-1) + " to 0");
				volt = connectEdge (tmpI, nx, volt);
			}

			//check up-right edge
			tmpI = (nx*ix)+iy+1;
			if (!record.Contains(tmpI) && tmpI % nx > 0 && res [tmpI] != 0 && res [tmpI] != 1) {
				volt [iup + 1] = 0;
				record.Add (tmpI);
				Debug.Log ("Changed volt " + (iup+1) + " to 0");
				volt = connectEdge (tmpI, nx, volt);
			}

			//check lower-left edge
			tmpI = (nx*(ix+1))+iy;
			if (!record.Contains(tmpI) && tmpI % nx > 0 && res [tmpI] != 0 && res [tmpI] != 1 && idn-1 >= 0) {
				volt [idn - 1] = 0;
				record.Add (tmpI);
				Debug.Log ("Changed volt " + (idn-1) + " to 0");
				volt = connectEdge (tmpI, nx, volt);
			}

			//check lower-right edge
			tmpI = (nx*(ix+1))+iy+1;
			if (!record.Contains(tmpI) && tmpI % nx > 0 && idn+1 < nx*(nx-1)+1 && res [tmpI] != 0 && res [tmpI] != 1) {
				volt [idn + 1] = 0;
				record.Add (tmpI);
				Debug.Log ("Changed volt " + (idn+1) + " to 0");
				volt = connectEdge (tmpI, nx, volt);
			}

			return volt;
		}
	}

	// Update is called once per frame
	void Update () {

	}

	//Code copied from network
	//Source: https://www.codeproject.com/Tips/388179/Linear-Equation-Solver-Gaussian-Elimination-Csharp
	private static double[] SolveLinearEquations(double[][] rows)
	{
	    int length = rows[0].Length;

	    for (int i = 0; i < rows.Length - 1; i++)
	    {
	        if (rows[i][i] == 0 && !Swap(rows, i, i))
	        {
	            return null;
	        }

	        for (int j = i; j < rows.Length; j++)
	        {
	            double[] d = new double[length];
	            for (int x = 0; x < length; x++)
	            {
	                d[x] = rows[j][x];
	                if (rows[j][i] != 0)
	                {
	                    d[x] = d[x] / rows[j][i];
	                }
	            }
	            rows[j] = d;
	        }

	        for (int y = i + 1; y < rows.Length; y++)
	        {
	            double[] f = new double[length];
	            for (int g = 0; g < length; g++)
	            {
	                f[g] = rows[y][g];
	                if (rows[y][i] != 0)
	                {
	                    f[g] = f[g] - rows[i][g];
	                }

	            }
	            rows[y] = f;
	        }
	    }

	    return CalculateResult(rows);
	}

	private static bool Swap(double[][] rows, int row, int column)
	{
	    bool swapped = false;
	    for (int z = rows.Length - 1; z > row; z--)
	    {
	        if (rows[z][row] != 0)
	        {
	            double[] temp = new double[rows[0].Length];
	            temp = rows[z];
	            rows[z] = rows[column];
	            rows[column] = temp;
	            swapped = true;
	        }
	    }

	    return swapped;
	}
	private static double[] CalculateResult(double[][] rows)
	{
	    double val = 0;
	    int length = rows[0].Length;
	    double[] result = new double[rows.Length];
	    for (int i = rows.Length - 1; i >= 0; i--)
	    {
	        val = rows[i][length - 1];
	        for (int x = length - 2; x > i - 1; x--)
	        {
	            val -= rows[i][x] * result[x];
	        }
	        result[i] = val / rows[i][i];

	        if (!IsValidResult(result[i]))
	        {
	            return null;
	        }
	    }
	    return result;
	}

	private static bool IsValidResult(double result)
	{
	    return result.ToString() != "NaN" || !result.ToString().Contains("Infinity");
	}  
}