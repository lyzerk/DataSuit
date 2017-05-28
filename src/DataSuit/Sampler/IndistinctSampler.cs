using System;
using System.Collections.Generic;
using DataSuit.Interfaces;

namespace DataSuit
{
    // Would be better with an IIndistinctSampler interface in between
    public class IndistinctSampler<T>: ISampler<T>
    {
        private List<T> data;

        public IndistinctSampler(List<T> _data)
        {
            setData(_data);
        }

        private void setData(List<T> _data)
        {
            data = _data;
        }

        private int getDataSize()
        {
            return data.Count;
        }

        public List<T> sample(int n){

            if( n >= getDataSize())
            {
                return data;
            }

            List<T> ret = new List<T>();
            Random generator = new Random();

            for(int i = 0 ; i < n ; i++)
            {
                ret.Add(data[generator.Next(0,n)]);
            }
            return ret;
        }
    }
}