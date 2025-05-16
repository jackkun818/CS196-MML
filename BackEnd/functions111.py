from typing import List, Dict, Any
from chains1111 import retrieval_chain,retrieval_chain_explain, retrieval_chain_guide,multi_query_chain,multi_query_chain_comfort, multi_query_chain_guide,hallucination_chain, fallback_chain, prescription_recommendation_chain, keyword_chain, recommened_paper_chain, multi_query_chain_explain, report_analysis_chain,select_relevance_guide, multi_query_chain_encourage
from vectorstores111 import dir, vectorstore_patient_evaluation, db, vectorstore_papers,training_db
import os
import re
import ast
from prompts111 import REVE_bg, PUME_bg
from fetch_pubmed_data111 import PubMedScraper
from langchain_community.document_loaders import CSVLoader
from structs111 import ReportAnalysis


def remove_specific_characters(text):
    """
    移除字符串中的特定字符。
    参数:
    text (str): 需要过滤的原始字符串。
    chars_to_remove (str or list): 需要移除的一个或多个字符组成的字符串或列表。
    返回:
    str: 过滤后的字符串，去除了指定的字符。
    """
    chars_to_remove=['\n','*','#',"{","}","\\","/","\\","-"]
    # 如果chars_to_remove不是一个集合，转换为集合
    if (isinstance(chars_to_remove, list)==False):
        raise ValueError("chars_to_remove should be a string, not a list")
    # 使用列表推导式来构建新的字符串，排除chars_to_remove中的字符
    filtered_text = ''.join([char for char in text if char not in chars_to_remove])

    return filtered_text

def answer_hallucination_check(question: str, docs: List[str]):
    generation = multi_query_chain.invoke({"question": question, "context": docs})
    result = hallucination_chain.invoke({"documents": docs, "generation": generation})
    if result.content.lower() == 'yes':
        return generation
    return generation
    # else:
    #     generatio
    # n = fallback_chain.invoke({"question": question})

def answer_hallucination_check_explain(params: dict, docs: List[str]):
    input_dict = {
        "params": params,
        "context": docs
    }
    generation = multi_query_chain_explain.invoke(input_dict)
    grade = 'no'
    counter = 0
    # while grade != 'yes':
    #     generation = multi_query_chain.invoke({"question": param.get("question"), "language": param.get("language"), "temperature": param.get("temperature"), "temperature": param.get("temperature"),"context": docs})
    #     result = hallucination_chain.invoke({"documents": docs, "generation": generation})
    #     grade = result.content.lower()
    #     counter += 1
    #     if counter > 35:
    #         break
    return generation

def answer_hallucination_check_comfort(params: dict, docs: List[str]):
    input_dict = {
        "params": params,
        "context": docs
    }
    generation = multi_query_chain_comfort.invoke(input_dict)
    grade = 'no'
    counter = 0
    return generation

def answer_hallucination_check_encourage(params: dict, docs: List[str]):
    input_dict = {
        "params": params,
        "context": docs
    }
    generation = multi_query_chain_encourage.invoke(input_dict)
    grade = 'no'
    counter = 0
    return generation

def answer_hallucination_check_guide(params: dict, docs: List[str]):
    input_dict = {
        "params": params,
        "context": docs
    }
    generation = multi_query_chain_guide.invoke(input_dict)
    grade = 'no'
    counter = 0
    return generation

def parse_docs(List: list):
    content =[]
    documents = List[0]
    print(documents)
    for document in documents:
        content.append(document.page_content)
    return content

# async def get_answer(param: dict):
#     docs = retrieval_chain.invoke({"question": param.get("question")})
#     answer = answer_hallucination_check_guide(param, docs)
#     return answer
async def get_answer(question:str):
    docs = retrieval_chain.invoke({"question": question})
    answer = answer_hallucination_check(question, docs)
    answer=remove_specific_characters(answer)
    return answer

async def get_explaination(params: dict):
    print("params={}".format(params))
    a=params.get("module_name")
    print("a={}".format(a))
    docs = retrieval_chain_explain.invoke({"module_name":params.get("module_name")})
    content = parse_docs(docs)
    explaination = answer_hallucination_check_explain(params, content)
    explaination = remove_specific_characters(explaination)
    return explaination

async def get_comfort(params: dict):
    #docs = retrieval_chain_comfort.invoke({"module_name":params.get("module_name"),"current_details": params.get("current_details")})
    content = ["遇到困难是正常的，你并不孤单，我们都在这里支持你。", "不要放弃，你的努力正在带来改变。", "我在这里支持你，我们一起克服这个挑战。"]
    comfort = answer_hallucination_check_comfort(params, content)
    comfort = remove_specific_characters(comfort)
    return comfort 

async def get_encourage(params: dict):
    content = ["你做得很好，每一步的进步都值得庆祝！", "你今天的表现非常棒，我看到你比上次进步了！", "你已经掌握了很多技巧，继续这样下去，你会恢复得更快。"]
    encourage = answer_hallucination_check_encourage(params, content)
    encourage = remove_specific_characters(encourage)
    return encourage 

async def get_guidance(params: dict):
    try:
        # 获取问题内容
        question = params.get("question")
        if not question:
            return "抱歉，我没有收到您的问题。请重新提问。"
            
        # 直接调用GPT API
        guidance = multi_query_chain_guide.invoke({"question": question})
        guidance = remove_specific_characters(guidance)
        return guidance
    except Exception as e:
        print(f"Error in get_guidance: {str(e)}")
        return f"抱歉，处理您的问题时出现了错误：{str(e)}"




def extract_params_from_message(message: str, function_id: int):
    print("message={}".format(message.content))
    if function_id == 1:
        match = re.search(r'module_name:(.*),patient_name:(.*), language:(.*)', message.content)
        print("match={}".format(match))
        if match:
            return {"module_name": match.group(1).strip(),"patient_name": match.group(2).strip(), "language": match.group(3).strip(),"module_id": function_id}
    if function_id == 2:
        match = re.match(r'module_name:(.*),patient_name:(.*), language:(.*), digital_person_name:(.*), relationship:(.*), patient_sex:(.*)',  message.content)
        if match:
            return {"module_name": match.group(1).strip(),"patient_name": match.group(2).strip(), "language": match.group(3).strip(),
                    "digital_person_name": match.group(4).strip(), "relationship": match.group(5).strip(), "patient_sex": match.group(6).strip(),"module_id": function_id}
    if function_id == 3:
        match = re.match(r'module_name:(.*),question:(.*), patient_name:(.*), language:(.*), digital_person_name:(.*), relationship:(.*), patient_sex:(.*)', message.content)
        if match:
            return {"module_name": match.group(1).strip(),"question":match.group(2).strip(),"patient_name": match.group(3).strip(), "language": match.group(4).strip(),
                    "digital_person_name": match.group(5).strip(), "relationship": match.group(6).strip(), "patient_sex": match.group(7).strip(),"module_id": function_id}    
    return {"question": "报错"}

def extract_similar_ids(similar_cases) -> List[str]:
    ids = []
    for docs in similar_cases:
        case = docs.page_content
        a=re.findall(r"\d+", case)
        id = re.findall(r"\d+", case)[0]
        # id = "{:05d}".format(int(id))
        ids.append(str(id))
    return tuple(ids)

def parse_result(result):
    pattern = re.compile(r'\{.*?\}')

    matches = pattern.findall(result)

    dict_list = [ast.literal_eval(match) for match in matches]

    return dict_list

def get_similar_ids(id):
    '''retrieve similar data based on patient ID
    '''
    query = f"SELECT * FROM patient_evaluation WHERE id = '{id}';"
    patient_data = db.run(query, include_columns=True)
    evaluation_retriever = vectorstore_patient_evaluation.as_retriever(search_kwargs={'k': 5})
    similar_cases = evaluation_retriever.invoke(patient_data)
    similar_ids = extract_similar_ids(similar_cases)
    return patient_data , similar_ids

def get_similar_cases(similar_ids):
    query = f"SELECT * FROM patient_evaluation WHERE id IN {similar_ids};"
    # print("similar_ids={}".format(similar_ids))
    similar_cases = db.run(query, include_columns=True)
    return similar_cases

def get_similar_cases_prescription(similar_ids):
    query = f"SELECT * FROM patient_prescription WHERE id IN {similar_ids};"
    similar_cases_prescription = db.run(query, include_columns=True)
    return similar_cases_prescription

def get_recommendation(id: str):
    patient_data, similar_ids = get_similar_ids(id)
    print("similar_ids={}".format(similar_ids))
    similar_cases = get_similar_cases(similar_ids)
    similar_cases_prescription = get_similar_cases_prescription(similar_ids)
    response = {}
    response['similar_cases'] = similar_cases
    response['recommendation'] = prescription_recommendation_chain.invoke({"patient_data": patient_data, "similar_cases_prescription": similar_cases_prescription})
    result=f"similar_cases:{response['similar_cases']},recommendation:{response['recommendation']}"
    result=remove_specific_characters(result)
    return result

def parse_papers(papers):
    parsed_papers = []
    for paper in papers:
        paper_dict = {}
        lines = paper.page_content.split('\n')
        for line in lines:
            try:
                key, value = line.split(": ", 1)
                paper_dict[key] = value
            except:
                pass
        parsed_papers.append(paper_dict)
    return parsed_papers

def get_paper_recommendation(description: str) -> str:
    keywords = keyword_chain.invoke({"description": description})
    PAPER_DIR = os.path.join(dir,'data/papers.csv')
    scraper = PubMedScraper(keywords=keywords, output = PAPER_DIR)
    scraper.main()

    loader = CSVLoader(
        file_path = PAPER_DIR,
        csv_args={
            "delimiter": ",",
            "quotechar": '"',
            "fieldnames": ['title', 'abstract', 'affiliations', 'authors', 'journal', 'date', 'keywords', 'url'],
        }
    )
    paper_documents = loader.load()

    vectorstore_papers.add_documents(documents = paper_documents)

    paper_retriever = vectorstore_papers.as_retriever(search_kwargs={'k': 5})
    retrieved_papers = paper_retriever.invoke(keywords)
    parsed_papers = parse_papers(retrieved_papers)
    responses = []
    for paper in parsed_papers:
        title, url, abstract = paper['title'], paper['url'], paper['abstract']
        response = recommened_paper_chain.invoke({"description": description, "abstract": abstract})
        response = f"Title: {title}; Reason of recommendation: {response} URL: {url}"
        responses.append(response)
    
    result='\n'.join(responses)
    result=remove_specific_characters(result)
    return result

def get_training_data(date, module):
    try:
        query = f"SELECT * FROM {module} WHERE 日期 = '{date}';"
        training_data = training_db.run(query, include_columns=True)
        return training_data
    except:
        raise ValueError("non-existent data")
    
def select_modular_prompt(module: str):
    if module == 'REVE':
        return REVE_bg
    elif module == 'PUME':
        return PUME_bg

def get_report_anaysis(query:ReportAnalysis):
    id, module, date = query.id, query.module, query.date
    data = get_training_data(date, module)
    module_info = select_modular_prompt(module)
    response = report_analysis_chain.invoke({"module_name": module, "module_info": module_info, 'data': data})
    response = remove_specific_characters(response)
    return response


# if __name__ == "__main__":
#     test_guidance()