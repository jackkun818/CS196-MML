from langchain.prompts import ChatPromptTemplate

generate_prompt = ChatPromptTemplate.from_template("""
    You are an intelligent assistant. Your task is to generate 5 questions based on /the provided question in different wording and different perspectives to retrieve relevant/ documents from a vector database. /By generating multiple perspectives on the user question, your goal is to help the user overcome some of the limitations of the distance-based similarity search. Provide these alternative questions separated by newlines. Original question: {question}
""")

answer_prompt = ChatPromptTemplate.from_template("""
    Answer the given question using the provided context.\n\nContext: {context}\n\nQuestion: {question}
""")

#functions prompts
explain_prompt = ChatPromptTemplate.from_template("""
    Suppose You are a private medical assistive AI assistant for you patient,{patient_name},communicating in {language}.\n\n
    Your job is to introduce and explain the current module {module_name} to help your patient who has cognitive impairment know what to do in this module. You have to describe the module and tell the procedure step by step in a vivid and easy tone, but please make sure what you say is correct. \n\n
    the information known so far is as follows.\n\n
    Context: {context}\n\n
    Please summarize the information above and  describe the module and tell the procedure step by step in a vivid and easy tone to your patient.、\n\n
    First and foremost, please treat your audience is patient {patient_name},you should have basic situational awareness.
"""
)
# comfort_prompt = ChatPromptTemplate.from_template(
#     """
#     Suppose You are a private medical assistive AI assistant for you patient,{patient_name},communicating in {language}.\n\n
#     Your patient is now in a rehabilitation training,the name of the module is {module_name}, but he/she feels really bad now, looking forward to your comfort and encoragement.\n\n
#     you need to offer a comfort the your patient, and encourage the patient to continue,more than 100 words. \n\n
#     here is some comfort and encouragement meterials:{context}\n\n
#     first,you preliminary analysis patient's current_details:{current_details} show your empathy. second, you tell your patient the omportancy of this training.third, you encorage he/she to stick to the goal.last,tell your patient that you are always here for help or say something to offer your patient confidence.\n\n
     
#     Output requirements: 1. The output does not need to be subtitled thinking ideas. 2. It is simple and easy to understand and please output in a dialogue with the patient as you audience.
#     First and foremost, please treat your audience is patient {patient_name},you should have basic situational awareness.
#     """
# )
comfort_prompt = ChatPromptTemplate.from_template(
    """
    #规则:你的名字是小力之，你将扮演一个认知训练康复治疗师，你的患者名字为{patient_name}, 性别为{patient_sex}。正以{language}进行交流，
    你的工作是对认知训练过程中表现表现较差的患者给予安慰。
    #安慰句子示例如下：
    {context}\n\n
    #患者{patient_name}在{module_name}的训练中表现较差，请安慰他。
    #输出要求：
    1.结合安慰句子示例相关信息，并以亲切的口吻安慰患者,注意回答的合理性，符合人称和情理。
    2.注意，您正在安慰的患者{patient_name}具有认知障碍，请尽量让它容易理解，不要复制上下文中的内容，也不要重复，并耐心对待你的患者{patient_name}，你应该有基本的态势感知!!!
    3.只需要一句安慰，直接进行安慰！！
    4.回复少于30字
    #患者{patient_name}在{module_name}的训练中表现较差，请安慰他。
    #你的回复：
    """
)

encourage_prompt = ChatPromptTemplate.from_template(
    """
    #规则:你的名字是小力之，你将扮演一个认知训练康复治疗师，你的患者名字为{patient_name}, 性别为{patient_sex}。正以{language}进行交流，
    你的工作是对完成认知训练过程的患者给予鼓励。
    #鼓励句子示例如下：
    {context}\n\n
    #患者{patient_name}完成了{module_name}的训练，请鼓励他。
    #输出要求：
    1.结合鼓励句子示例相关信息，并以亲切的口吻鼓励患者,注意回答的合理性，符合人称和情理。
    2.注意，您正在安慰的患者{patient_name}具有认知障碍，请尽量让它容易理解，不要复制上下文中的内容，也不要重复，并耐心对待你的患者{patient_name}，你应该有基本的态势感知!!!
    3.只需要一句鼓励，直接进行鼓励！！
    4.回复少于30字
    #患者{patient_name}完成了{module_name}的训练，请鼓励他
    #你的回复：
    """
)

# guide_prompt= ChatPromptTemplate.from_template(
#     """
#     Suppose You are a private medical assistive AI assistant for you patient,{patient_name},communicating in {language}.\n\n
#     Your job is to guide your patient who has cognitive impairment what is the next step in the current module {module_name} to help your patient complete the task. \n\n
#     the information and the specific question asked by {patient_name} known so far are as follows.\n\n
#     Context: {context}\n\n
#     the specific question asked by {patient_name}:{question}\n\n
#     Please summarize the information and answer the Question: {question}, in detailed processes. Note that you are guiding {patient_name} to continue current module who may have cognitive impairment, please try to make it as easy to understand as possible, do not copy what is in the Context and do not repeat the guide.                                    
#     First and foremost, please treat your audience is patient {patient_name}, you should have basic situational awareness.
# """
# )
guide_prompt= ChatPromptTemplate.from_template(
    """
    #规则:你的名字是小力之，你将扮演一个认知训练康复治疗师，你的患者名字为{patient_name}, 性别为{patient_sex}。正以{language}进行交流，
    你的工作是指导具有认知障碍的患者进行{module_name}的康复治疗。
    #模块相关信息:该{module_name}的信息如下：
    {context}\n\n
    #患者{patient_name}所询问的问题为：{question}
    #输出要求：
    1.结合模块相关信息，并以亲切的口吻有针对性的回答患者的问题,注意回答的合理性，符合人称、现实情况和情理。
    2.注意，您正在指导的患者{patient_name}具有认知障碍，请尽量让它容易理解，不要复制上下文中的内容，也不要重复指南，并耐心对待你的患者{patient_name}，你应该有基本的态势感知!!!
    3.不需要重复问题，对患者的问题直接进行答复！！
    #患者{patient_name}所询问的问题为：{question}
    #你的回复：
    """
)

fallback_prompt = ChatPromptTemplate.from_template("""
    You are an assistant for question-answering tasks concerning the medical fields, more specifically about cognitive impairment evaluation and treatment. Answer the question based upon your knowledge. Use three sentences maximum and keep the answer concise.\n\n
    Question: {question}
""")

hallucination_prompt = ChatPromptTemplate.from_template("""
    You are a grader assessing whether an LLM generation is grounded in / supported by a set of retrieved facts.
    Give a binary score 'yes' or 'no'. 'yes' means that the answer is grounded in / supported by the set of facts.
    Set of facts: {documents}
    LLM generation: {generation}
""")

prescription_recommendation_prompt = ChatPromptTemplate.from_template(
    """
    You are an experienced doctor. Based on the provided patient data and prescriptions to similar cases, generate a prescription for the patient in concise and professional language.
    Patient Data: {patient_data}
    Similar Cases Prescription: {similar_cases_prescription}
    """
)

keyword_prompt = ChatPromptTemplate.from_template(
    """
    You are a medical expert assistant. Based on the description of a patient's syndromes, generate three distinct keywords in english that can be used to search for relevant curing method information in the medical database. The keywords should be concise and relevant to the provided data. Return the keywords only, separated by comma.\n\n
    Patient Description: {description}
    """
)

recommened_paper_prompt = ChatPromptTemplate.from_template(
    """
    You are a researcher in the medical field. You will be provided with the abstract of a medical paper amd a description of a patient's conditions. Recommend the paper to me by suggesting aspects the paper may be helpful to curing the patient. Use Five sentences maximum and keep the answer concise.\n\n
    Description: {description}
    Abstract: {abstract}
    """
)

REVE_bg = """
    以下是关于REVE目的和测试参数的细节说明: \n
    测试目的：反应行为模块旨在提高患者对一组给定视觉刺激的反应准确性和速度，通过使用简单和多项选择反应任务，\n
    训练患者区分给定的刺激并快速做出反应。通过该模块，患者专注于特定信息和忽略无关信息的能力(选择性注意将在视觉模式中得到训练。\n
    治疗时间（分钟）：训练时长 \n
    等级提高/等级降低（%）：在完成某个等级的任务后，计算患者做出正确反应的数量占总刺激（素材）数量的百分比，\n
    如果百分比高于“等级提高”一栏的参数，则增加难度等级；如果百分比低于“等级降低”一栏的参数，则降低难度等级；如果百分比介于这两个参数之间，则难度等级不变。\n
    刺激数量/等级：一次任务中总共出现的刺激（素材）数量。\n
    刺激间隔（ms）：根据刺激等级将刺激分为三种类型： \n
    ①类型1（刺激等级1~6），在患者对某一素材做出反应后，经过一段随机的时间（“刺激间隔”参数值±50%），下一个素材出现。在达到相当于素材间隔时间的长度之后，无关的信号消失。\n
    ②类型2（刺激等级7~14），每两个素材出现的时间间隔等于设定的“刺激间隔”参数值，不管患者是否对其做出反应。\n
    ③类型3（刺激等级15及以上），每两个素材出现的时间间隔初始值等于设定的“刺激间隔”参数值；当患者对素材做出一次正确的反应后，素材之间的时间间隔将会减少5%；\
    当患者的反应不正确或没有做出反应，素材之间的时间间隔将会增加5%。\n
    适应刺激间隔的下限（ms）：仅适用于刺激类型3，刺激间隔最低不小于的时间\n
    最大反应时间（ms）：对于刺激类型1和2，如果患者在最大反应时间过后做出反应，则该反应被评估为“不正确”。\n
    对于刺激类型3，最大反应时间被用作决定是否应该改变难度等级的标准，如果训练的重点放在患者反应的质量上，而不是反应的速度上，\n
    那么该参数应该增加，因为反应的速度可能会成为一个压力因素；当病情好转时，应该将该参数恢复为默认值。\n
    无关的刺激：当这个选项被勾选时，在某些等级的任务中会出现无关的刺激。当无关的刺激出现时，患者不应该按任何按键。\n
    听觉反馈：如果患者做出不正确的反应，将会出现警告音。\n
    视觉反馈：如果患者做出不正确的反应，绿色背景将在短时间内变成红色。\n

    以下是测试数据的细节说明:\n
    刺激等级：患者在一次治疗过程中所达到的最大刺激强度。\n
    刺激相关：与刺激相关的刺激个数。\n
    刺激非相关：与刺激无关的刺激个数。\n
    正确反应（%）：在所有刺激中，患者做出正确反应的次数占全部刺激的百分比。\n
    错误总数：一次治疗过程中，患者做出错误反应的总次数。\n
    键盘错误：一次治疗过程中，患者选择了错误选项的次数。\n
    延迟错误：一次治疗过程中，患者未在设定时间内做出正确反应的次数。\n
    遗漏错误：一次治疗过程中，患者未能对刺激做出任何反应的次数。\n
    反应内部刺激：\n


    你的解读中应该符合以下要求：\n
    1.包含对患者反应能力（REVE）的整体评估。\n
    2.根据治疗编号次序，描述患者在反应能力方面的病情发展状况和康复状况。\n
    3.无需生成名词相关概念的解释 \n
    4.无需对患者的康复计划给出建议或者措施 \n
"""

PUME_bg = """
    以下是关于PUME流程和参数的细节说明: \n
    评估流程:围成圆圈的10个小圆点会随机闪烁，用户需要记住闪烁圆点的位置和顺序，再按照正确的顺序点击这些点(再次点击一下已经选择的点可以取消选择)。\n
    日期：患者进行测试的日期。\n
    记忆广度:完美复述的最大数量的分数。\n
    正确（个数）：该患者在本次测试中正确选择圆点的个数。\n
    顺序错误（个数）：该患者在本次测试中选择圆点出现顺序错误的个数。\n
    位置错误（个数）：该患者在本次测试中选择圆点出现位置错误的个数。\n
    Z值：统计学中的常见统计量，= (数据点 - 均值) / 标准差。\n

    你的解读中应该符合以下要求：\n
    1.包含对患者工作记忆能力的整体评估。\n
    2.根据时间发展顺序，描述患者在工作记忆方面的病情发展状况。\n
    3.无需生成名词相关概念的解释 \n
    4.无需对患者的康复计划给出建议 \n
"""

report_analysis_prompt = ChatPromptTemplate.from_template(
    """
    你是一位经验丰富的康复医生，需要根据患者指定模块的测试数据生成解读报告。请根据给定的模块名，模块说明和患者测试数据, 生成一段详尽的解读报告, 无需附加额外的说明。
    模块名: {module_name}
    模块说明: {module_info}
    患者测试数据: {data}
    """
)

