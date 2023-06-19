from main3256_3267_3276_joblib import main
import orjson
import pandas as pd
from tqdm import tqdm

if __name__ == "__main__":
    path = "./output/verification/"
    verificationData, submitresAll = main(verification=True,percentage=0.2,savaPath=path)

    # with open(path+'submitresAll.json', 'r') as f:
    #     submitresAll = orjson.load(f)
    #
    # with open(path+'verificationData.json', 'r') as f:
    #     verificationData = orjson.load(f)

    # Create mappings
    submit_dict = {item['triple_id']: item['candidate_voter_list'] for item in tqdm(submitresAll,desc="submitresAll")}
    verification_dict = {item['triple_id']: item['voter_id'] for item in tqdm(verificationData,desc="verificationData")}

    # Calculate scores and add them to verificationData
    for item in tqdm(verificationData,desc="verificationData"):
        triple_id = item['triple_id']
        if str(triple_id) in submit_dict and triple_id in verification_dict:
            true_voter = verification_dict[triple_id]
            candidate_voters = submit_dict[str(triple_id)]
            for i, candidate_voter in enumerate(candidate_voters):
                if candidate_voter == true_voter:
                    item['MRR'] = 1 / (i + 1)
                    item['HITS'] = 1 if i < 5 else 0
                    break
            else:
                item['MRR'] = 0
                item['HITS'] = 0

    # Save as json
    with open(path+'mrr.json', 'wb') as f:
        orjson.dumps(verificationData, f)

    # Save as Excel
    df = pd.DataFrame(verificationData)
    df.to_excel(path+'mrr.xlsx', index=False)