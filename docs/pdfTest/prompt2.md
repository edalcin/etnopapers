TAREFA: Extrair APENAS dados encontrados no documento. NÃO invente, infira ou gere dados.

Se uma informação NÃO está no documento, retorne null. NÃO use "N/A", "desconhecido" ou valores padrão.
Se a informação ESTÁ no documento, copie EXATAMENTE como aparece.

RETORNE UM JSON VÁLIDO COM ESTA ESTRUTURA:
{
"titulo": "copie o título exato do documento",
"autores": ["Copie nomes exatamente como aparecem"],
"ano": número inteiro (ex: 2010),
"resumo": "em português brasileiro - copie ou traduza APENAS o que existe",
"especies": [{
"nome_vernacular": "nome comum encontrado no texto",
"nome_cientifico": "nome científico encontrado no texto",
"tipo_uso": "medicinal/alimentar/ritual etc - APENAS se explicitamente mencionado",
"parte_usada": "folhas/raiz/sementes etc - APENAS se explicitamente mencionado",
"preparacao": "chá/decocção/etc - APENAS se explicitamente mencionado"
}],
"comunidade": {"nome": null, "localizacao": null},
"pais": null,
"estado": null,
"municipio": null,
"local": null,
"bioma": null,
"metodologia": null,
"ano_coleta": null
}

REGRAS ABSOLUTAS:
1. COPIE EXATAMENTE do documento. NÃO reescreva, NÃO resuma, NÃO interprete.
2. SE NÃO ESTÁ NO DOCUMENTO → use null. NUNCA invente valores plausíveis.
3. titulo: copie palavra por palavra do documento
4. autores: copie os nomes EXATAMENTE como aparecem. NÃO reformate para ABNT se não estiverem nesse formato no documento.
5. ano: se disser "2010", extraia 2010. Se disser "Recebido: 7 de abril de 2010", extraia 2010. NÃO adivinhe.
6. resumo: copie o resumo/abstract que existe. Se não houver, use null. NÃO gere um resumo novo.
7. especies: APENAS plantas que estão NOMEADAS e DESCRITAS no documento. NÃO invente plantas.
8. comunidade/pais/estado/municipio/local: COPIE APENAS o que está escrito. Null se não encontrado.
9. Retorne APENAS o JSON, sem markdown (sem ```json), sem explicações.
10. NUNCA complete campos faltantes com inferências. Melhor retornar null do que um valor inventado.