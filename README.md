> [CAAI-BDSC2023社交图谱链接预测 任务二：社交图谱动态链接预测__天池大赛-阿里云天池 (aliyun.com)](https://tianchi.aliyun.com/competition/entrance/532074/introduction)

1. 大赛概况介绍

---------

### CAAI-BDSC2023 大会背景

CAAI 第八届全国大数据与社会计算学术会议（China National Conference on Big Data & Social Computing，简称 BDSC2023）将于 2023 年 7 月 9-11 日在新疆乌鲁木齐召开，由中国人工智能学会主办、社会计算与社会智能专委、新疆工程学院共同承办。本届会议的主题为 “数字化转型与可持续发展”，立足全球数字化转型的技术变革、治理与政策实践，通过跨学科交叉视野探索通过数字化推动可持续发展的全球经验与中国智慧。

为了促进交叉学科汇聚与融合，本届大会将开展 CAAI-BDSC2023 社会计算创新大赛。本次大赛将围绕大会主题 "数字化转型与可持续发展"，旨在通过社会计算增进自我认知、在社会智能中结识他人，以此实现数字社会的健康、和谐、可持续发展！

CAAI-BDSC2023 社会计算创新大赛在天池平台特别设置 “社交图谱链接预测” 赛道。

### "社交图谱链接预测" 赛道背景

社会网络是由社会个体成员之间因为互动而形成的相对稳定的社会结构，成员之间的互动和联系进一步影响人们的社会行为，电子商务平台大范围的普及和使用，不仅满足人们丰富多样的消费需求，也承载着社会成员基于商品消费产生的互动链接，形成基于电商平台的在线社交网络，电商场景社交知识图谱的构建有助于深入理解在线社交网络的结构特性与演化机理，为用户社交属性识别和互动规律发现提供有效方式。电商平台活动和场景形式丰富多样，用户表现出不同的社交行为偏好，且伴随活动场景、互动对象、互动方式、互动时间的不同而不断发生变化，动态性高，不确定性强，这些都给社交知识图谱的构建和应用带来巨大挑战。

本赛道基于阿里电子商务平台用户互动数据展开社交图谱链接预测任务，本次评测包括两个子任务：社交图谱小样本场景链接预测，社交图谱动态链接预测，任务评测依托[阿里巴巴天池平台](https://tianchi.aliyun.com/)展开，两个评测任务具体内容在赛题与数据章节介绍。

2. 赛程安排（Schedule）

-----------------

本次大赛分为报名组队、初赛和决赛三个阶段，具体安排和要求如下：  
报名组队——————3 月 20 日—6 月 16 日 初赛阶段——————4 月 1 日—6 月 16 日 复赛阶段——————6 月 20 日—6 月 25 日 颁奖阶段——————7 月 16 日 - 7 月 17 日

**报名组队与实名认证（2023 年 3 月 22 日—6 月 16 日）**

*   报名方式：3 月 22 日[阿里天池平台](https://tianchi.aliyun.com/)将开放本次比赛的组队报名、登录比赛官网，完成个人信息注册，即可报名参赛；选手可以单人参赛，也可以组队参赛。组队参赛的每个团队 2-3 人，每位选手只能加入一支队伍；
*   选手需确保报名信息准确有效，组委会有权取消不符合条件队伍的参赛资格及奖励；
*   选手报名、组队变更等操作截止时间为 6 月 16 日 23：59：59；各队伍（包括队长及全体队伍成员）需要在 6 月 16 日 23：59：59 前完成实名认证（认证入口：天池官网 - 右上角个人中心 - 认证 - 支付宝实名认证），**未完成认证的参赛团队将无法进行后续的比赛**。

大赛官方钉钉群：  
扫描以下二维码加入，最新通知将会第一时间在群内同步：  
![](https://img.alicdn.com/imgextra/i3/O1CN01vz0LQZ23UDTAOwotq_!!6000000007258-0-tps-380-476.jpg)

初赛阶段（2023 年 **4 月 1 日 - 2023 年 6 月 16 日，UTC+8）**  
初赛的几个关键时间点：

*   4 月 1 号天池平台将开放竞赛数据集和系统测评。选手报名成功后，参赛队伍通过天池平台下载数据，本地调试算法，在线提交结果。
*   初赛提供训练数据集，供参赛选手训练算法模型；同时提供测试数据集，供参赛选手提交评测结果，参与排名。初赛阶段提交格式在具体任务章节有详细介绍。
*   初赛时间为 2023 年 4 月 1 日 - 2023 年 6 月 16 日，系统每天提供 **2 次**评测机会，系统进行实时评测并返回成绩，排行榜每小时进行更新，按照评测指标从高到低排序。排行榜将选择参赛队伍在本阶段的历史最优成绩进行排名展示。
*   初赛淘汰：2023 年 6 月 16 日上午 9：59：59，初赛阶段未产出成绩的队伍将被取消复赛参赛资格。
*   初赛结束，初赛排名**前 30 名**的参赛队伍将进入复赛，复赛名单将在 6 月 19 日 21：59：59 之前公布。

**复赛阶段（2023 年 6 月 20 日—2023 年 6 月 25 日，UTC+8）**

*   复赛阶段提供复赛测试数据集，参赛选手提交评测结果，参与排名。复赛阶段提交规范和初赛阶段保持一致。
*   复赛时间为 2023 年 6 月 20 日 - 2023 年 6 月 25 日。本阶段，系统每天提供 3 次实时评测，每小时更新排行榜，按照评测指标从高到低排序。排行榜将选择参赛队伍在本阶段的历史最优成绩进行排名展示。复赛提交截止时间 6 月 25 日 17：59：59。
*   复赛结束后，大赛组织方将通知成绩排名前 3 名的队伍提交代码，赛题组织方将对所提交代码进行审核，要求模型能复现出最优提交成绩。对于未提交、复现未成功或审核不通过的队伍，将取消决赛资格，并通知递补队伍，榜单将在官方完成代码审核后公布。

**决赛评选与颁奖（2023 年 7 月 16 日—2023 年 7 月 17 日，UTC+8）**  
复赛排名前 3 名队伍需注册参会，到会议现场进行领奖（颁奖时间为 2023 年 7 月 16 日），无法注册参会的队伍视为自动放弃，取消决赛获奖资格，并通知递补队伍。参与决赛评奖的队伍需提交报告 ppt 参与决赛评选（6 月 30 日前提交），由评委会裁定最终获奖名单，评委会将在获奖队伍中推荐 2 支队伍参与会议现场汇报展示（汇报时间为 2023 年 7 月 17 日）。

3. 参赛规则

-------

1.  所有参赛选手都必须在天池平台管理系统中注册，本次比赛的参赛对象仅限全日制在校大学生（本科、硕士、博士均可）和企业员工；
2.  参赛选手需确保注册时提交信息准确有效，所有的比赛资格及奖金支付均以提交信息为准；
3.  参赛选手在管理系统中组队，参赛队伍成员数量不得超过 3 个，报名截止日期之后不允许更改队员名单；
4.  每支队伍需指定一名队长，队伍名称不超过 15 个字符，队伍名的设定不得违反中国法律法规或公序良俗词汇，否则组织者有可能会解散队伍；
5.  每名选手只能参加一支队伍，一旦发现某选手以注册多个账号的方式参加多支队伍，将取消相关队伍的参赛资格；
6.  允许使用开源代码或工具，但不允许使用任何未公开发布或需要授权的代码或工具；
7.  参赛队伍可在参赛期间随时上传验证集的预测结果，管理系统会定时更新各队伍的最新排名情况。

8.  奖金设置（奖品和奖励）

--------------

<table><thead><tr><th _msttexthash="6140602" _msthash="208">奖项</th><th _msttexthash="4282746" _msthash="209">奖励</th></tr></thead><tbody><tr><td _msttexthash="10858523" _msthash="210">一等奖 1 名</td><td _msttexthash="28334124" _msthash="211">荣誉证书 + 奖金 5000 元</td></tr><tr><td _msttexthash="10871393" _msthash="212">二等奖 2 名</td><td _msttexthash="28333760" _msthash="213">荣誉证书 + 奖金 3000 元</td></tr><tr><td _msttexthash="10859602" _msthash="214">三等奖 3 名</td><td _msttexthash="28333396" _msthash="215">荣誉证书 + 奖金 1000 元</td></tr><tr><td _msttexthash="13552032" _msthash="216">优秀作品奖</td><td _msttexthash="13544154" _msthash="217">荣誉证书</td></tr></tbody></table>

5. 组织和合作伙伴（Organizations）

-------------------------

![](https://img.alicdn.com/imgextra/i2/O1CN01cRJEYc20Bwp2roBmm_!!6000000006812-0-tps-1552-720.jpg)

任务描述
----

商品分享是用户在电商平台中主要的社交互动形式，商品分享行为包含了用户基于商品的社交互动偏好信息，用户的商品偏好和社交关系随时间变化，其基于商品的社交互动属性也随时间改变。本任务关注社交图谱动态链接预测问题，用四元组 （u， i， v， t） 表示用户在 t （time） 时刻的商品分享互动行为，其中 i （item） 标识特定商品，u （user） 表示发起商品链接分享的邀请用户，v （voter） 表示接收并点击该商品链接的回流用户。因此在本任务中链接预测指的是，在已知邀请用户 u，商品 i 和时间 t 的情况下，预测对应回流用户 v。

任务目标
----

针对社交图谱动态链接预测，参赛队伍需要根据已有动态社交图谱四元组数据，对于给定 u，i，t，对 v 进行预测。

数据集介绍
-----

### 初赛训练集 & 测试集

在初赛阶段，我们会发布用户动态商品分享数据作为训练集。训练集由三部分构成，可以在天池平台下载获取。

*   item_share_train_info.json：用户动态商品分享数据，每行是一个 json 串，具体字段信息如下：

<table><thead><tr><th><strong _msttexthash="4995445" _msthash="268">字段</strong></th><th><strong _msttexthash="12583701" _msthash="269">字段说明</strong></th></tr></thead><tbody><tr><td _msttexthash="96109" _msthash="270">user_id</td><td _msttexthash="13891397" _msthash="271">邀请用户 ID</td></tr><tr><td _msttexthash="94471" _msthash="272">item_id</td><td _msttexthash="9401041" _msthash="273">分享商品 ID</td></tr><tr><td _msttexthash="116051" _msthash="274">voter_id</td><td _msttexthash="17299672" _msthash="275">选民的用户 ID</td></tr><tr><td _msttexthash="9308897" _msthash="276">时间戳</td><td _msttexthash="29834571" _msthash="277">分享行为发生时间</td></tr></tbody></table>

*   user_info.json：用户信息数据，每行是一个 jsonl 串，具体字段信息如下：

<table><thead><tr><th><strong _msttexthash="4995445" _msthash="279">字段</strong></th><th><strong _msttexthash="12583701" _msthash="280">字段说明</strong></th></tr></thead><tbody><tr><td _msttexthash="96109" _msthash="281">user_id</td><td _msttexthash="81314493" _msthash="282">用户的 ID，包含数据集中所有的 inviter_id 和 voter_id</td></tr><tr><td _msttexthash="181688" _msthash="283">user_gender</td><td _msttexthash="175346587" _msthash="284">用户性别，0 表示女性用户，1 表示男性用户，未知为 - 1</td></tr><tr><td _msttexthash="113750" _msthash="285">user_age</td><td _msttexthash="276977467" _msthash="286">用户所在年龄段，数值范围为 1～8，数值越大表示年龄越大，未知为 - 1</td></tr><tr><td _msttexthash="160381" _msthash="287">user_level</td><td _msttexthash="319972133" _msthash="288">用户的平台积分水平，数值范围为 1~10，数值越大表示用户的平台积分水平越高</td></tr></tbody></table>

*   item_info.json：商品信息数据，每行是一个 json 串，具体字段信息如下：

<table><thead><tr><th><strong _msttexthash="4995445" _msthash="290">字段</strong></th><th><strong _msttexthash="12583701" _msthash="291">字段说明</strong></th></tr></thead><tbody><tr><td _msttexthash="94471" _msthash="292">item_id</td><td _msttexthash="10843846" _msthash="293">商品编号</td></tr><tr><td _msttexthash="92664" _msthash="294">cate_id</td><td _msttexthash="19127966" _msthash="295">商品叶子类目 ID</td></tr><tr><td _msttexthash="241930" _msthash="296">cate_level1_id</td><td _msttexthash="20125534" _msthash="297">商品一级类目 ID</td></tr><tr><td _msttexthash="111488" _msthash="298">brand_id</td><td _msttexthash="22954620" _msthash="299">商品所属的品牌 ID</td></tr><tr><td _msttexthash="95693" _msthash="300">shop_id</td><td _msttexthash="24847810" _msthash="301">商品所属的店铺 ID</td></tr></tbody></table>

在初赛阶段，我们会发布测试集 item_share_ preliminary_test_info.json，每行是一个 json 串，具体字段信息如下：

<table><thead><tr><th><strong _msttexthash="4995445" _msthash="303">字段</strong></th><th><strong _msttexthash="12583701" _msthash="304">字段说明</strong></th></tr></thead><tbody><tr><td _msttexthash="96109" _msthash="305">user_id</td><td _msttexthash="13891397" _msthash="306">邀请用户 ID</td></tr><tr><td _msttexthash="94471" _msthash="307">item_id</td><td _msttexthash="9401041" _msthash="308">分享商品 ID</td></tr><tr><td _msttexthash="9308897" _msthash="309">时间戳</td><td _msttexthash="29834571" _msthash="310">分享行为发生时间</td></tr></tbody></table>

### 复赛测试集

在复赛阶段我们发布复赛测试集 item_share_final_test_info.json，复赛结果提交方式和初赛保持一致，具体参见任务提交说明章节。

任务提交说明
------

参赛队伍可在初赛和复赛阶段下载对应的测试集数据。每一行包括 triple_id， inviter_id， item_id， timestamp。

测试集示例如下： {“triple_id”：“0”， “inviter_id”：“0”， “item_id”：“12423”，

“timestamp”：“2023-01-05 10：11：  
12”

}

本次比赛的目标对给定的邀请者 inviter_id，物品 item_id 和时间信息时间戳，预测此次交互行为的回流者 voter_id。参赛队伍需要从用户信息数据 （见 user_info.json） 中选择用户，给出一个长度为 5 的候选回流者 （voter_id） 列表，其中预测概率大的 voter 排在前面，把对应的 user_id 填入 candidate_voter_list，最后以 json 文件的形式提交。初赛和复赛的提交文件形式相同。

提交文件示例如下：

```
{
	"triple_id":"0",
	"candidate_voter_list": ["1", "2", "3", "4", "5"]
}


```

评价指标
----

本任务采用 MRR（Mean Reciprocal Rank，平均倒数排名）指标来评估模型的预测效果。

对于一个查询：（inviter， item， ？， timestamp ），我们需要根据概率找出可能的候选选民列表，为 candidate_voter_list，概率高的排在前面。若真实的回流者排在第 n 位，则该次 query 的分数为 1/n。由于候选 voter 列表长度为 5，所以此处 n 最大为 5，如果列表中不包含真实的回流者，则此次查询的得分为 0。最终对多个查询的分数求平均，计算公式如下：

M R R=\frac{1}{|Q|}\sum_{i=1}^{|Q|}\frac{1}{{rank}_i}

其中，为测试 query 集合，![](https://intranetproxy.alipay.com/skylark/lark/__latex/4ef7132d0df72d9e3db76f6391960a3d.svg)![](https://intranetproxy.alipay.com/skylark/lark/__latex/e0855a1809001422510099ea59ad348c.svg)为其中 query 的个数，  
\frac{1}{\operatorname{rank}_i}= \begin{cases}\frac{1}{i} & \text { （真实回流者排在第 } i \text { 位） } \\ 0 & \text {（ 候选选民列表中没有真实的回流者）}\end{cases}

### 基线模型

<table><thead><tr><th></th><th _msttexthash="41028" _msthash="324">MRR@5</th><th _msttexthash="52182" _msthash="325">HITS@5</th></tr></thead><tbody><tr><td><a href="https://github.com/meteor-gif/BDSC_Task_2" target="_blank" _msttexthash="5423990" _msthash="326">基线</a></td><td _msttexthash="46085" _msthash="327">0.03437</td><td _msttexthash="47060" _msthash="328">0.09258</td></tr></tbody></table>

