from langchain.prompts import PromptTemplate

guide_prompt = PromptTemplate(
    template="""You are a helpful AI assistant. Please answer the following question:

Question: {question}

Please provide a clear and concise answer.""",
    input_variables=["question"]
) 