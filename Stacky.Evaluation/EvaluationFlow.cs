namespace Stacky.Evaluation;

public static class EvaluationFlow
{
    public static EvaluationState If(EvaluationState state)
    {
        state = state.Pop(out var trueValue);
        if (trueValue is not EvaluationValue.Function trueFunc)
        {
            throw new InvalidCastException($"Expected arg 0 to be Function but got {trueValue}");
        }

        state = state.Pop(out var condition);
        if (condition is not EvaluationValue.Boolean conditionBool)
        {
            throw new InvalidCastException($"Expected arg 1 to be Boolean but got {condition}");
        }

        return conditionBool.Value ? Evaluator.RunExpression(state, trueFunc.Body) : state;
    }

    public static EvaluationState IfElse(EvaluationState state)
    {
        state = state.Pop(out var falseValue);
        if (falseValue is not EvaluationValue.Function falseFunc)
        {
            throw new InvalidCastException($"Expected arg 0 to be Function but got {falseValue}");
        }
        
        state = state.Pop(out var trueValue);
        if (trueValue is not EvaluationValue.Function trueFunc)
        {
            throw new InvalidCastException($"Expected arg 1 to be Function but got {trueValue}");
        }

        state = state.Pop(out var condition);
        if (condition is not EvaluationValue.Boolean conditionBool)
        {
            throw new InvalidCastException($"Expected arg 2 to be Boolean but got {condition}");
        }

        return Evaluator.RunExpression(state, conditionBool.Value ? trueFunc.Body : falseFunc.Body);
    }
    
    public static EvaluationState While(EvaluationState state)
    {
        state = state.Pop(out var loopBody);
        if (loopBody is not EvaluationValue.Function loopBodyFunc)
        {
            throw new InvalidCastException($"Expected arg 0 to be Function but got {loopBody}");
        }
        
        state = state.Pop(out var conditionBody);
        if (conditionBody is not EvaluationValue.Function conditionBodyFunc)
        {
            throw new InvalidCastException($"Expected arg 1 to be Function but got {conditionBody}");
        }

        while (true)
        {
            state = Evaluator.RunExpression(state, conditionBodyFunc.Body);
            
            state = state.Pop(out var condition);
            if (condition is not EvaluationValue.Boolean conditionBool)
            {
                throw new InvalidCastException($"Expected arg 2 to be Boolean but got {condition}");
            }

            if (!conditionBool.Value)
            {
                break;
            }

            state = Evaluator.RunExpression(state, loopBodyFunc.Body);
        }

        return state;
    }
}