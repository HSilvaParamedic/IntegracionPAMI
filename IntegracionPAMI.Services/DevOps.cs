using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DevOps
{
    public enum result
    {
        OK,
        Warning,
        Error
    }
    public result Resultado { get; set; }
    public string DescripcionError { get; set; }

    public DevOps(result pResultado, string pDescripcionError)
    {
        this.Resultado = pResultado;
        this.DescripcionError = pDescripcionError;
    }

    public DevOps(string pDescripcionError)
    {
        this.Resultado = result.Error;
        this.DescripcionError = pDescripcionError;
    }

    public DevOps()
    {
        this.Resultado = result.OK;
        this.DescripcionError = "";
    }

}