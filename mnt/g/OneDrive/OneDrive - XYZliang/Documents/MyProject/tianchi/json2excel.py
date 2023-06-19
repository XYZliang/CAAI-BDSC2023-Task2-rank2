'''
Author: ZhangLiang 1424625705@qq.com
Date: 2023-05-25 10:12:57
LastEditors: ZhangLiang 1424625705@qq.com
LastEditTime: 2023-05-25 10:13:35
FilePath: /tianchi/json2excel
Description: 这是默认设置,请设置`customMade`, 打开koroFileHeader查看配置 进行设置: https://github.com/OBKoro1/koro1FileHeader/wiki/%E9%85%8D%E7%BD%AE
'''
import os
import pandas as pd
import orjson
import openpyxl

# 指定数据目录
data_dir = './data'

# 遍历文件目录
for filename in os.listdir(data_dir):
    # 检查是否是.json文件
    if filename.endswith('.json'):
        with open(os.path.join(data_dir, filename), 'rb') as file:
            # 使用orjson加载json数据
            data = orjson.loads(file.read())

            # 将json数据转为pandas DataFrame
            df = pd.DataFrame(data)

            # 转为excel文件，替换文件后缀名为.xlsx
            excel_filename = filename.replace('.json', '.xlsx')
            df.to_excel(os.path.join(data_dir, excel_filename))

print('JSON files have been successfully converted to Excel files.')
