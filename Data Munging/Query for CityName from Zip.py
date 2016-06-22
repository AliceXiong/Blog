from pyzipcode import Pyzipcode as pz
import pandas as pd

def cityLookup(zip):
    #zip = repr(zip)
    return pz.get(zip,"US")['city']


f_lines = open('C:\\Users\\axiong\\OneDrive for Business\\00 MPI\\Ref_zip.txt','rb').readlines()
solved = []

for line in f_lines:
    line = line.decode('utf8').rstrip('\r\n')
    try:
        s = cityLookup(line)
    except:
        s = 'null'
    solved.append(s)

len(solved)
my_df = pd.DataFrame(solved)
my_df.to_csv('my_csv.csv', index=False, header=False)
