import React, { useState, useEffect } from "react";
import { Routes, Route ,useLocation} from "react-router-dom"
import AuthCallback from "./components/AuthCallBack";
import Login from "./components/Login";
import Home from "./home";
function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const [isLoading,setIsLoading] = useState(true)
  const [user, setUser] = useState(null)
  const location = useLocation()



  async function getUserInfo() {
    try {
    let response = await fetch('http://localhost:5000/api/me', {
      method: "GET",
      credentials: "include"
    })
    if (response.ok) {
      let jsonResponse = await response.json()
      let user = jsonResponse.user
      return user
    }
    return null
  }
  catch(err){
    setIsLoggedIn(false)
    setIsLoading(false)
  }
  }



  useEffect(() => {

     async function fetchUser (){
      let user = await getUserInfo()
      if (user != null) {
        setUser(user)
        setIsLoggedIn(true)
      }
      else {
        setIsLoggedIn(false)
      }
      setIsLoading(false)

    
    }
    fetchUser();
    



  }, [location])



  if(isLoading){
    return (
      <div style={{
        height:"100vh",
        fontSize:"40px",
         display:"flex",  
         alignItems:"center" 
         ,justifyContent:"center"
        }
         }>
        loading ...
      </div>
    )
  }


  return (

    <Routes>
      <Route path="/" element={isLoggedIn && !isLoading ? <Home user={user} /> : <Login />} />
      <Route path="/auth/callback" element={<AuthCallback setIsLoggedIn = {setIsLoggedIn} setIsLoading = {setIsLoading}/>} />
    </Routes>
  )
}

export default App;