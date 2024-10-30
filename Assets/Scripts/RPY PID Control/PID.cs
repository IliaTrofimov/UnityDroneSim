namespace RPY_PID_Control
{
	[System.Serializable]
	public class PID 
	{
		private float integral, lastError;
		public float pFactor, iFactor, dFactor;
	
		public PID(float pFactor, float iFactor, float dFactor)
		{
			this.pFactor = pFactor;
			this.iFactor = iFactor;
			this.dFactor = dFactor;
		}
	
		public float Update(float setpoint, float actual, float timeFrame)
		{
			var error = setpoint - actual;
			integral += error * timeFrame;
			
			var deriv = (error - lastError) / timeFrame;
			lastError = error;
			
			var final = error*pFactor + integral*iFactor + deriv*dFactor;
			if (final > -0.1f && final < 0.1f)
				final = 0f;
			return final;
		}
	}
}
