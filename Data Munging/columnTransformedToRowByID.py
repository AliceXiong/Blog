import pandas as pd
import numpy as np
CPT=pd.read_csv("CPT.txt", sep = "\t")
CPT1=CPT.groupby('ID837')['ProcedureCode'].apply(list)
CPT1.to_csv("CPT1.csv")

#df.groupby('a')['b'].apply(list).tolist()