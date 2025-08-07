import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useEffect, useState } from "react";

function AuthCallback(props) {

  const [searchParams, setSearchParams] = useSearchParams();
  const [logginState, setLogginState] = useState("Loggin You In ...")
  const navigate = useNavigate();
  const url = import.meta.env.VITE_API_URL




 





  async function SendCodeToBackend(code) {
    try {
      let response = await fetch(`${url}/callBack`, {
        method: "POST",
        credentials: "include",
        headers: {
          "Content-Type": "application/json",

        },
        body: JSON.stringify({ Code: code })
      })
      if (!response.ok) {
        let result = await response.json();
        throw new Error(result.message || "Unknown error");
      }
      return response


    }
    catch (err) {
      alert(err.message)
      setLogginState("loggin failed")
    }


  }


  async function AuthUser(code) {

    let response = await SendCodeToBackend(code)
    if ( response && response.ok) {
      props.setIsLoading(true);
       props.setIsLoggedIn(true)
        navigate("/")
    }
    else {
      setLogginState("loggin failed")
    }


  }



  useEffect(() => {

    let code = searchParams.get("code")
    if (code) {
      AuthUser(code)
    }

  }, []);

  return <div>
    <h2 style={{ alignItems: "center", display: "flex", justifyContent: "center" }}>{logginState}</h2>


  </div>;
}

export default AuthCallback;