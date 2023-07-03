import orjson as json
from tqdm import tqdm

with open(r"G:\OneDrive\OneDrive - XYZliang\Documents\MyProject\CAAI-BDSC2023-2-net6\submit.json", 'rb') as f:
    test1_data = json.loads(f.read())

with open(r"G:\OneDrive\OneDrive - XYZliang\Desktop\submit.json", 'rb') as f:
    test2_data = json.loads(f.read())

print(len(test1_data))
print(len(test2_data))



for i in tqdm(range(len(test1_data))):
    test1 = test1_data[i]
    test2 = test2_data[i]
    id = test1['triple_id']
    testList1 = test1['candidate_voter_list']
    testList2 = test2['candidate_voter_list']
    if len(testList1) != len(testList2):
        print("error"+id+"长度")
    for j in range(len(testList1)):
        if testList1[j] != testList2[j]:
            print("error:"+id)
            print(test1)
            print(test2)